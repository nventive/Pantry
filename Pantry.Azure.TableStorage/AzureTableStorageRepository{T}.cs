using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Azure.TableStorage.Queries;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Logging;
using Pantry.Mapping;
using Pantry.Queries;

namespace Pantry.Azure.TableStorage
{
    /// <summary>
    /// Azure Table Storage Repository Implementation.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class AzureTableStorageRepository<T> : IRepository<T>
        where T : class, IIdentifiable
    {
        private readonly CloudTableFor<T> _cloudTableFor;
        private readonly IIdGenerator<T> _idGenerator;
        private readonly ITableStorageKeysResolver<T> _keysResolver;
        private readonly IMapper<T, DynamicTableEntity> _tableEntityMapper;
        private readonly IEnumerable<IAzureTableStorageQueryHandler> _queryHandlers;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageRepository{T}"/> class.
        /// </summary>
        /// <param name="cloudTableFor">The <see cref="CloudTableFor{T}"/> instance to use.</param>
        /// <param name="idGenerator">The <see cref="IIdGenerator{T}"/>.</param>
        /// <param name="tableEntityMapper">The mapper to <see cref="ITableEntity"/>.</param>
        /// <param name="queryHandlers">The available query handlers.</param>
        /// <param name="keysResolver">The <see cref="ITableStorageKeysResolver{T}"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public AzureTableStorageRepository(
            CloudTableFor<T> cloudTableFor,
            IIdGenerator<T> idGenerator,
            ITableStorageKeysResolver<T> keysResolver,
            IMapper<T, DynamicTableEntity> tableEntityMapper,
            IEnumerable<IAzureTableStorageQueryHandler> queryHandlers,
            ILogger<AzureTableStorageRepository<T>>? logger = null)
        {
            _cloudTableFor = cloudTableFor ?? throw new ArgumentNullException(nameof(cloudTableFor));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _keysResolver = keysResolver ?? throw new ArgumentNullException(nameof(keysResolver));
            _tableEntityMapper = tableEntityMapper ?? throw new ArgumentNullException(nameof(tableEntityMapper));
            _queryHandlers = queryHandlers ?? Enumerable.Empty<IAzureTableStorageQueryHandler>();
            _logger = logger ?? NullLogger<AzureTableStorageRepository<T>>.Instance;
        }

        /// <inheritdoc/>
        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = await _idGenerator.Generate(entity);
            }

            var tableEntity = _tableEntityMapper.MapToDestination(entity);

            try
            {
                var operationResult = await _cloudTableFor.CloudTable.ExecuteAsync(
                    TableOperation.Insert(tableEntity),
                    cancellationToken)
                    .ConfigureAwait(false);

                var result = _tableEntityMapper.MapToSource((DynamicTableEntity)operationResult.Result);
                _logger.LogAdded(
                    entityType: typeof(T),
                    entityId: result.Id,
                    entity: result);

                return result;
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == 409)
            {
                var conflictException = new ConflictException(typeof(T).Name, entity.Id, storageException);
                _logger.LogAddedWarning(
                    entityType: typeof(T),
                    entityId: entity.Id,
                    warning: "Conflict",
                    exception: conflictException);
                throw conflictException;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<T?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var (partitionKey, rowKey) = _keysResolver.GetStorageKeys(id);
            var operationResult = await _cloudTableFor.CloudTable.ExecuteAsync(
                TableOperation.Retrieve(partitionKey, rowKey),
                cancellationToken)
                .ConfigureAwait(false);

            var result = operationResult.HttpStatusCode == 404
                ? null
                : _tableEntityMapper.MapToSource((DynamicTableEntity)operationResult.Result);
            _logger.LogGetById(
                    entityType: typeof(T),
                    entityId: id,
                    entity: result);

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                var targetUpdatedTableEntity = _tableEntityMapper.MapToDestination(entity);
                targetUpdatedTableEntity.ETag = "*";
                var operationResult = await _cloudTableFor.CloudTable.ExecuteAsync(
                    TableOperation.Replace(targetUpdatedTableEntity),
                    cancellationToken)
                    .ConfigureAwait(false);

                var result = _tableEntityMapper.MapToSource((DynamicTableEntity)operationResult.Result);
                _logger.LogUpdated(
                    entityType: typeof(T),
                    entityId: result.Id,
                    entity: result);
                return result;
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == 404)
            {
                var exception = new NotFoundException(typeof(T).Name, entity.Id);
                _logger.LogUpdatedWarning(
                    entityType: typeof(T),
                    entityId: entity.Id,
                    warning: "NotFound",
                    exception: exception);

                throw exception;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryDeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var (partitionKey, rowKey) = _keysResolver.GetStorageKeys(id);
            var result = await _cloudTableFor.CloudTable.ExecuteAsync(
                TableOperation.Retrieve(partitionKey, rowKey),
                cancellationToken)
                .ConfigureAwait(false);

            if (result.HttpStatusCode == 404)
            {
                _logger.LogDeletedWarning(
                    entityType: typeof(T),
                    entityId: id,
                    warning: "NotFound");
                return false;
            }

            await _cloudTableFor.CloudTable.ExecuteAsync(
                TableOperation.Delete((ITableEntity)result.Result),
                cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDeleted(
                entityType: typeof(T),
                entityId: id);

            return true;
        }

        /// <inheritdoc/>
        public Task<IContinuationEnumerable<TResult>> FindAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var targetQueryHandlerType = typeof(IAzureTableStorageQueryHandler<,,>)
                .MakeGenericType(typeof(T), typeof(TResult), query.GetType());
            var handler = _queryHandlers.FirstOrDefault(x => targetQueryHandlerType.IsInstanceOfType(x));

            if (handler is null)
            {
                throw new UnsupportedFeatureException($"Query {query} has no handler for repository {this}.");
            }

            var executeMethod = handler.GetType().GetMethod("Execute");

            if (executeMethod is null)
            {
                throw new InternalErrorException($"Unable to find Execute method on query handler {handler}.");
            }

            return (Task<IContinuationEnumerable<TResult>>)executeMethod.Invoke(
                handler,
                new object[] { query, _cloudTableFor, _tableEntityMapper, cancellationToken });
        }
    }
}
