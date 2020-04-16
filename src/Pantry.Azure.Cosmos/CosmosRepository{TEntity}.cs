using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Generators;
using Pantry.Logging;
using Pantry.Providers;
using Pantry.Queries;
using Pantry.Queries.Criteria;
using SqlKata.Compilers;

namespace Pantry.Azure.Cosmos
{
    /// <summary>
    /// Cosmos Repository Implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class CosmosRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IIdentifiable, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="cosmosContainerFor">The <see cref="CosmosContainerFor{TEntity}"/>.</param>
        /// <param name="idGenerator">The <see cref="IIdGenerator{TEntity}"/>.</param>
        /// <param name="timestampProvider">The <see cref="ITimestampProvider"/>.</param>
        /// <param name="mapper">The <see cref="ICosmosEntityMapper{TEntity}"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CosmosRepository(
            CosmosContainerFor<TEntity> cosmosContainerFor,
            IIdGenerator<TEntity> idGenerator,
            ITimestampProvider timestampProvider,
            ICosmosEntityMapper<TEntity> mapper,
            ILogger<CosmosRepository<TEntity>>? logger = null)
        {
            CosmosContainerFor = cosmosContainerFor ?? throw new ArgumentNullException(nameof(cosmosContainerFor));
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            TimestampProvider = timestampProvider ?? throw new ArgumentNullException(nameof(timestampProvider));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            Logger = logger ?? NullLogger<CosmosRepository<TEntity>>.Instance;
        }

        /// <summary>
        /// Gets the <see cref="CosmosContainerFor{TEntity}"/>.
        /// </summary>
        protected CosmosContainerFor<TEntity> CosmosContainerFor { get; }

        /// <summary>
        /// Gets the <see cref="IIdGenerator{T}"/>.
        /// </summary>
        protected IIdGenerator<TEntity> IdGenerator { get; }

        /// <summary>
        /// Gets the <see cref="ITimestampProvider"/>.
        /// </summary>
        protected ITimestampProvider TimestampProvider { get; }

        /// <summary>
        /// Gets the <see cref="ICosmosEntityMapper{TEntity}"/>.
        /// </summary>
        protected ICosmosEntityMapper<TEntity> Mapper { get; }

        /// <summary>
        /// Gets the <see cref="Container"/>.
        /// </summary>
        protected Container Container => CosmosContainerFor.Container;

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the SQL query <see cref="Compiler"/>.
        /// </summary>
        private CosmosDbQueryCompiler QueryCompiler { get; } = new CosmosDbQueryCompiler();

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

            if (entity is ITimestamped timestampedEntity && timestampedEntity.Timestamp is null)
            {
                timestampedEntity.Timestamp = TimestampProvider.CurrentTimestamp();
            }

            var document = Mapper.MapToDestination(entity);
            try
            {
                var documentResult = await Container.CreateItemAsync(
                    document,
                    partitionKey: Mapper.GetPartitionKey(document.Id),
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var result = Mapper.MapToSource(documentResult.Resource);

                Logger.LogAdded(
                    entityType: typeof(TEntity),
                    entityId: result.Id,
                    entity: result);

                return result;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                var conflictException = new ConflictException(typeof(TEntity).Name, entity.Id, cosmosEx);
                Logger.LogAddedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "Conflict",
                    exception: cosmosEx);
                throw conflictException;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<(TEntity, bool)> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.Id))
            {
                return (await AddAsync(entity, cancellationToken).ConfigureAwait(false), true);
            }

            var existingEntity = await TryGetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            if (existingEntity is null)
            {
                return (await AddAsync(entity, cancellationToken).ConfigureAwait(false), true);
            }
            else
            {
                return (await UpdateAsync(entity, cancellationToken).ConfigureAwait(false), false);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var documentResult = await Container.ReadItemAsync<CosmosDocument>(
                    id,
                    Mapper.GetPartitionKey(id),
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var result = Mapper.MapToSource(documentResult.Resource);

                Logger.LogGetById(
                    entityType: typeof(TEntity),
                    entityId: id,
                    entity: result);

                return result;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogGetById(
                    entityType: typeof(TEntity),
                    entityId: id,
                    entity: null);

                return null;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var targetDocument = Mapper.MapToDestination(entity);

            try
            {
                ItemRequestOptions? itemRequestsOptions = null;
                if (entity is IETaggable taggable && !string.IsNullOrEmpty(taggable.ETag))
                {
                    itemRequestsOptions = new ItemRequestOptions
                    {
                        IfMatchEtag = taggable.ETag,
                    };
                }

                var documentResult = await Container.ReplaceItemAsync(
                    targetDocument,
                    targetDocument.Id,
                    partitionKey: Mapper.GetPartitionKey(targetDocument.Id),
                    requestOptions: itemRequestsOptions,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var result = Mapper.MapToSource(documentResult.Resource);

                Logger.LogUpdated(
                    entityType: typeof(TEntity),
                    entityId: result.Id,
                    entity: result);
                return result;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                var exception = new NotFoundException(typeof(TEntity).Name, entity.Id, cosmosEx);
                Logger.LogUpdatedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "NotFound",
                    exception: exception);

                throw exception;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                var exception = new ConcurrencyException(typeof(TEntity).Name, entity.Id, cosmosEx.Message, cosmosEx);
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
            if (string.IsNullOrEmpty(id))
            {
                Logger.LogDeletedWarning(
                    entityType: typeof(TEntity),
                    entityId: "(null)",
                    warning: "NotFound");

                return false;
            }

            try
            {
                await Container.DeleteItemAsync<CosmosDocument>(
                id,
                Mapper.GetPartitionKey(id),
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

                Logger.LogDeleted(
                    entityType: typeof(TEntity),
                    entityId: id);

                return true;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogDeletedWarning(
                    entityType: typeof(TEntity),
                    entityId: id,
                    warning: "NotFound",
                    exception: cosmosEx);
                return false;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = 50, CancellationToken cancellationToken = default)
        {
            if (limit <= 0)
            {
                return ContinuationEnumerable.Empty<TEntity>();
            }

            var iterator = Container.GetItemLinqQueryable<CosmosDocument>(
                continuationToken: continuationToken,
                requestOptions: new QueryRequestOptions { MaxItemCount = limit })
                .Where(x => x.EntityType == Mapper.GetEntityType())
                .ToFeedIterator();

            var response = await iterator.ReadNextAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var result = response.Resource.Select(x => Mapper.MapToSource(x)).ToContinuationEnumerable(response.ContinuationToken);

            Logger.LogFind(new FindAllQuery<TEntity> { ContinuationToken = continuationToken, Limit = limit }, result);

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (query.Limit <= 0)
            {
                return ContinuationEnumerable.Empty<TEntity>();
            }

            var queryBuilder = new SqlKata.Query("e")
                .Select("*")
                .Where($"e[\"{CosmosDocument.TypeAttribute}\"]", Mapper.GetEntityType());

            string QuotedMappedPropertyPath(string propertyPath)
            {
                var mappedPropertyPath = Mapper.ResolveQueryPropertyPaths(propertyPath);
                return $"e{string.Join(string.Empty, mappedPropertyPath.Split(".").Select(x => $"[\"{x}\"]"))}";
            }

            foreach (var criterion in query)
            {
                queryBuilder = criterion switch
                {
                    EqualToPropertyCriterion equalTo => queryBuilder.Where(QuotedMappedPropertyPath(equalTo.PropertyPath), equalTo.Value),
                    GreaterThanPropertyCriterion gt => queryBuilder.Where(QuotedMappedPropertyPath(gt.PropertyPath), ">", gt.Value),
                    GreaterThanOrEqualToPropertyCriterion gte => queryBuilder.Where(QuotedMappedPropertyPath(gte.PropertyPath), ">=", gte.Value),
                    LessThanPropertyCriterion lt => queryBuilder.Where(QuotedMappedPropertyPath(lt.PropertyPath), "<", lt.Value),
                    LessThanOrEqualToPropertyCriterion lte => queryBuilder.Where(QuotedMappedPropertyPath(lte.PropertyPath), ">=", lte.Value),
                    StringContainsPropertyCriterion strCont => queryBuilder.WhereRaw($"CONTAINS({QuotedMappedPropertyPath(strCont.PropertyPath)}, ?)", strCont.Value),
                    OrderCriterion order => queryBuilder.OrderByRaw($"{QuotedMappedPropertyPath(order.PropertyPath)} {(order.Ascending ? "ASC" : "DESC")}"),
                    _ => throw new UnsupportedFeatureException($"The {criterion} criterion is not supported by {this}."),
                };
            }

            var queryDefinition = QueryCompiler.ToQueryDefinition(queryBuilder);
            Logger.LogTrace("FindAsync() {CosmosDbQueryText}", queryDefinition.QueryText);

            var response = await Container.GetItemQueryIterator<CosmosDocument>(
                queryDefinition,
                continuationToken: query.ContinuationToken,
                requestOptions: new QueryRequestOptions { MaxItemCount = query.Limit })
                .ReadNextAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var result = response.Resource.Select(x => Mapper.MapToSource(x)).ToContinuationEnumerable(response.ContinuationToken);

            Logger.LogFind(query, result);

            return result;
        }
    }
}
