using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        /// <param name="continuationTokenEncoder">The <see cref="IContinuationTokenEncoder{TableContinuationToken}"/>.</param>
        /// <param name="keysResolver">The <see cref="IAzureTableStorageKeysResolver{T}"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public AzureTableStorageRepository(
            CloudTableFor<TEntity> cloudTableFor,
            IIdGenerator<TEntity> idGenerator,
            IAzureTableStorageKeysResolver<TEntity> keysResolver,
            IMapper<TEntity, DynamicTableEntity> tableEntityMapper,
            IContinuationTokenEncoder<TableContinuationToken> continuationTokenEncoder,
            ILogger<AzureTableStorageRepository<TEntity>>? logger = null)
        {
            CloudTableFor = cloudTableFor ?? throw new ArgumentNullException(nameof(cloudTableFor));
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            KeysResolver = keysResolver ?? throw new ArgumentNullException(nameof(keysResolver));
            TableEntityMapper = tableEntityMapper ?? throw new ArgumentNullException(nameof(tableEntityMapper));
            ContinuationTokenEncoder = continuationTokenEncoder ?? throw new ArgumentNullException(nameof(continuationTokenEncoder));
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
        /// Gets the <see cref="IContinuationTokenEncoder{TableContinuationToken}"/>.
        /// </summary>
        protected IContinuationTokenEncoder<TableContinuationToken> ContinuationTokenEncoder { get; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
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

            string? sentEtag = null; // To properly catch ETag format exceptions.
            try
            {
                var targetUpdatedTableEntity = TableEntityMapper.MapToDestination(entity);
                sentEtag = targetUpdatedTableEntity.ETag;
                if (string.IsNullOrEmpty(targetUpdatedTableEntity.ETag))
                {
                    targetUpdatedTableEntity.ETag = "*";
                }

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
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == 412)
            {
                var exception = new ConcurrencyException(typeof(TEntity).Name, entity.Id, storageException.Message);
                Logger.LogUpdatedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "Concurrency",
                    exception: exception);

                throw exception;
            }
            catch (ArgumentException argumentException) when (argumentException.Message.Contains("ETag", StringComparison.InvariantCultureIgnoreCase))
            {
                var exception = new ConcurrencyException(typeof(TEntity).Name, entity.Id, argumentException.Message);
                Logger.LogUpdatedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "Concurrency",
                    exception: exception);

                throw exception;
            }
            catch (StorageException storageException) when (storageException.Message.Contains(sentEtag, StringComparison.InvariantCultureIgnoreCase))
            {
                var exception = new ConcurrencyException(typeof(TEntity).Name, entity.Id, storageException.Message);
                Logger.LogUpdatedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "Concurrency",
                    exception: exception);

                throw exception;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default)
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
        public Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = Query.DefaultLimit, CancellationToken cancellationToken = default)
        {
            if (limit <= 0)
            {
                return Task.FromResult(ContinuationEnumerable.Empty<TEntity>());
            }

            return PrepareQueryAndExecuteAsync(
                new FindAllQuery<TEntity> { ContinuationToken = continuationToken, Limit = limit },
                _ => { },
                cancellationToken);
        }

        /// <summary>
        /// Prepares a <see cref="TableQuery{DynamicTableEntity}"/> for execution,
        /// allows its customization trough <paramref name="apply"/>, and then
        /// executes it and map out the result.
        /// </summary>
        /// <param name="query">The original query.</param>
        /// <param name="apply">Applies customization to the table query.</param>
        /// <param name="cancellationToken">the <see cref="CancellationToken"/>.</param>
        /// <returns>The mapped out results.</returns>
        protected virtual async Task<IContinuationEnumerable<TEntity>> PrepareQueryAndExecuteAsync(
            IQuery<TEntity> query,
            Action<TableQuery<DynamicTableEntity>> apply,
            CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (apply is null)
            {
                throw new ArgumentNullException(nameof(apply));
            }

            var tableQuery = new TableQuery<DynamicTableEntity>();
            tableQuery.Take(query.Limit);
            apply(tableQuery);

            var operationResult = await CloudTableFor.CloudTable.ExecuteQuerySegmentedAsync(
                tableQuery,
                await ContinuationTokenEncoder.Decode(query.ContinuationToken),
                cancellationToken).ConfigureAwait(false);

            var result = operationResult.Results
                .Select(x => TableEntityMapper.MapToSource(x))
                .ToContinuationEnumerable(await ContinuationTokenEncoder.Encode(operationResult.ContinuationToken));

            Logger.LogFind(query, result);

            return result;
        }
    }
}
