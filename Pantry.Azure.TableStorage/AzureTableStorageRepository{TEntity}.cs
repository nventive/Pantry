using System;
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
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class AzureTableStorageRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageRepository{T}"/> class.
        /// </summary>
        /// <param name="cloudTableFor">The <see cref="CloudTableFor{T}"/> instance to use.</param>
        /// <param name="idGenerator">The <see cref="IIdGenerator{T}"/>.</param>
        /// <param name="tableEntityMapper">The mapper to <see cref="ITableEntity"/>.</param>
        /// <param name="queryHandlerExecutor">The query handler executor.</param>
        /// <param name="keysResolver">The <see cref="IAzureTableStorageKeysResolver{T}"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public AzureTableStorageRepository(
            CloudTableFor<TEntity> cloudTableFor,
            IIdGenerator<TEntity> idGenerator,
            IAzureTableStorageKeysResolver<TEntity> keysResolver,
            IMapper<TEntity, DynamicTableEntity> tableEntityMapper,
            IQueryHandlerExecutor<TEntity, IAzureTableStorageQueryHandler> queryHandlerExecutor,
            ILogger<AzureTableStorageRepository<TEntity>>? logger = null)
        {
            CloudTableFor = cloudTableFor ?? throw new ArgumentNullException(nameof(cloudTableFor));
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            KeysResolver = keysResolver ?? throw new ArgumentNullException(nameof(keysResolver));
            TableEntityMapper = tableEntityMapper ?? throw new ArgumentNullException(nameof(tableEntityMapper));
            QueryHandlerExecutor = queryHandlerExecutor ?? throw new ArgumentNullException(nameof(queryHandlerExecutor));
            Logger = logger ?? NullLogger<AzureTableStorageRepository<TEntity>>.Instance;
        }

        /// <summary>
        /// Gets the <see cref="CloudTableFor{T}"/>.
        /// </summary>
        protected CloudTableFor<TEntity> CloudTableFor { get; }

        /// <summary>
        /// Gets the <see cref="IIdGenerator{T}"/>.
        /// </summary>
        protected IIdGenerator<TEntity> IdGenerator { get; }

        /// <summary>
        /// Gets the <see cref="IAzureTableStorageKeysResolver{T}"/>.
        /// </summary>
        protected IAzureTableStorageKeysResolver<TEntity> KeysResolver { get; }

        /// <summary>
        /// Gets the <see cref="IMapper{T, DynamicTableEntity}"/>.
        /// </summary>
        protected IMapper<TEntity, DynamicTableEntity> TableEntityMapper { get; }

        /// <summary>
        /// Gets the <see cref="IQueryHandlerExecutor{TEntity, IAzureTableStorageQueryHandler}"/>.
        /// </summary>
        protected IQueryHandlerExecutor<TEntity, IAzureTableStorageQueryHandler> QueryHandlerExecutor { get; }

        /// <summary>
        /// Gets the <see cref="ILogger{T}"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc/>
        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = await IdGenerator.Generate(entity);
            }

            var tableEntity = TableEntityMapper.MapToDestination(entity);

            try
            {
                var operationResult = await CloudTableFor.CloudTable.ExecuteAsync(
                    TableOperation.Insert(tableEntity),
                    cancellationToken)
                    .ConfigureAwait(false);

                var result = TableEntityMapper.MapToSource((DynamicTableEntity)operationResult.Result);
                Logger.LogAdded(
                    entityType: typeof(TEntity),
                    entityId: result.Id,
                    entity: result);

                return result;
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == 409)
            {
                var conflictException = new ConflictException(typeof(TEntity).Name, entity.Id, storageException);
                Logger.LogAddedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "Conflict",
                    exception: conflictException);
                throw conflictException;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var (partitionKey, rowKey) = KeysResolver.GetStorageKeys(id);
            var operationResult = await CloudTableFor.CloudTable.ExecuteAsync(
                TableOperation.Retrieve(partitionKey, rowKey),
                cancellationToken)
                .ConfigureAwait(false);

            var result = operationResult.HttpStatusCode == 404
                ? null
                : TableEntityMapper.MapToSource((DynamicTableEntity)operationResult.Result);
            Logger.LogGetById(
                    entityType: typeof(TEntity),
                    entityId: id,
                    entity: result);

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                var targetUpdatedTableEntity = TableEntityMapper.MapToDestination(entity);
                targetUpdatedTableEntity.ETag = "*";
                var operationResult = await CloudTableFor.CloudTable.ExecuteAsync(
                    TableOperation.Replace(targetUpdatedTableEntity),
                    cancellationToken)
                    .ConfigureAwait(false);

                var result = TableEntityMapper.MapToSource((DynamicTableEntity)operationResult.Result);
                Logger.LogUpdated(
                    entityType: typeof(TEntity),
                    entityId: result.Id,
                    entity: result);
                return result;
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == 404)
            {
                var exception = new NotFoundException(typeof(TEntity).Name, entity.Id);
                Logger.LogUpdatedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "NotFound",
                    exception: exception);

                throw exception;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryDeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var (partitionKey, rowKey) = KeysResolver.GetStorageKeys(id);
            var result = await CloudTableFor.CloudTable.ExecuteAsync(
                TableOperation.Retrieve(partitionKey, rowKey),
                cancellationToken)
                .ConfigureAwait(false);

            if (result.HttpStatusCode == 404)
            {
                Logger.LogDeletedWarning(
                    entityType: typeof(TEntity),
                    entityId: id,
                    warning: "NotFound");
                return false;
            }

            await CloudTableFor.CloudTable.ExecuteAsync(
                TableOperation.Delete((ITableEntity)result.Result),
                cancellationToken)
                .ConfigureAwait(false);

            Logger.LogDeleted(
                entityType: typeof(TEntity),
                entityId: id);

            return true;
        }

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TResult>> FindAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return QueryHandlerExecutor.ExecuteAsync<TResult, IQuery<TResult>>(query);
        }
    }
}
