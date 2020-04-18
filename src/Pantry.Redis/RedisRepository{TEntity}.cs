using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Generators;
using Pantry.Logging;
using Pantry.Providers;
using Pantry.Queries;
using StackExchange.Redis;

namespace Pantry.Redis
{
    /// <summary>
    /// Redis Repository Implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class RedisRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IIdentifiable, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="databaseFor">The <see cref="RedisDatabaseFor{TEntity}"/>.</param>
        /// <param name="idGenerator">The <see cref="IIdGenerator{TEntity}"/>.</param>
        /// <param name="etagGenerator">The <see cref="IETagGenerator{T}"/>.</param>
        /// <param name="timestampProvider">The <see cref="ITimestampProvider"/>.</param>
        /// <param name="mapper">The <see cref="IRedisEntityMapper{TEntity}"/>.</param>
        /// <param name="continuationTokenEncoder">The <see cref="IContinuationTokenEncoder{TContinuationToken}"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public RedisRepository(
            RedisDatabaseFor<TEntity> databaseFor,
            IIdGenerator<TEntity> idGenerator,
            IETagGenerator<TEntity> etagGenerator,
            ITimestampProvider timestampProvider,
            IRedisEntityMapper<TEntity> mapper,
            IContinuationTokenEncoder<LimitOffsetContinuationToken> continuationTokenEncoder,
            ILogger<RedisRepository<TEntity>>? logger = null)
        {
            DatabaseFor = databaseFor ?? throw new ArgumentNullException(nameof(databaseFor));
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            EtagGenerator = etagGenerator ?? throw new ArgumentNullException(nameof(etagGenerator));
            TimestampProvider = timestampProvider ?? throw new ArgumentNullException(nameof(timestampProvider));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            ContinuationTokenEncoder = continuationTokenEncoder ?? throw new ArgumentNullException(nameof(continuationTokenEncoder));
            Logger = logger ?? NullLogger<RedisRepository<TEntity>>.Instance;
        }

        /// <summary>
        /// Gets the <see cref="RedisDatabaseFor{TEntity}"/>.
        /// </summary>
        protected RedisDatabaseFor<TEntity> DatabaseFor { get; }

        /// <summary>
        /// Gets the <see cref="IIdGenerator{T}"/>.
        /// </summary>
        protected IIdGenerator<TEntity> IdGenerator { get; }

        /// <summary>
        /// Gets the <see cref="IETagGenerator{T}"/>.
        /// </summary>
        protected IETagGenerator<TEntity> EtagGenerator { get; }

        /// <summary>
        /// Gets the <see cref="ITimestampProvider"/>.
        /// </summary>
        protected ITimestampProvider TimestampProvider { get; }

        /// <summary>
        /// Gets the <see cref="IRedisEntityMapper{TEntity}"/>.
        /// </summary>
        protected IRedisEntityMapper<TEntity> Mapper { get; }

        /// <summary>
        /// Gets the <see cref="IContinuationTokenEncoder{LimitOffsetContinuationToken}"/>.
        /// </summary>
        protected IContinuationTokenEncoder<LimitOffsetContinuationToken> ContinuationTokenEncoder { get; }

        /// <summary>
        /// Gets the <see cref="IDatabase"/>.
        /// </summary>
        protected IDatabase Database => DatabaseFor.Database;

        /// <summary>
        /// Gets an <see cref="IServer"/> to use for querying.
        /// </summary>
        protected virtual IServer ServerForQuerying => Database.Multiplexer.GetServer(Database.Multiplexer.GetEndPoints().Last());

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

            if (entity is IETaggable taggableEntity && string.IsNullOrEmpty(taggableEntity.ETag))
            {
                taggableEntity.ETag = await EtagGenerator.Generate(entity);
            }

            if (entity is ITimestamped timestampedEntity && timestampedEntity.Timestamp is null)
            {
                timestampedEntity.Timestamp = TimestampProvider.CurrentTimestamp();
            }

            var key = Mapper.GetRedisKey(entity.Id);
            var hashEntries = Mapper.MapToDestination(entity).ToArray();

            var tran = Database.CreateTransaction();
            tran.AddCondition(Condition.KeyNotExists(key));
#pragma warning disable CS4014 // We can't await this one, as it is not "executed"; this is the way the API is built.
            tran.HashSetAsync(key, hashEntries);
#pragma warning restore CS4014

            var executed = await tran.ExecuteAsync().ConfigureAwait(false);
            if (!executed)
            {
                var conflictException = new ConflictException(typeof(TEntity).Name, entity.Id);
                Logger.LogAddedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "Conflict",
                    exception: conflictException);
                throw conflictException;
            }

            var result = Mapper.MapToSource(hashEntries);

            Logger.LogAdded(
                entityType: typeof(TEntity),
                entityId: result.Id,
                entity: result);

            return result;
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
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var key = Mapper.GetRedisKey(id);
            var hashEntries = await Database.HashGetAllAsync(key).ConfigureAwait(false);

            if (!hashEntries.Any())
            {
                Logger.LogGetById(
                    entityType: typeof(TEntity),
                    entityId: id,
                    entity: null);
                return null;
            }

            var result = Mapper.MapToSource(hashEntries);
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

            string? entryETag = null;
            if (entity is IETaggable taggableEntity)
            {
                entryETag = taggableEntity.ETag;
                taggableEntity.ETag = await EtagGenerator.Generate(entity);
            }

            if (entity is ITimestamped timestampedEntity && timestampedEntity.Timestamp is null)
            {
                timestampedEntity.Timestamp = TimestampProvider.CurrentTimestamp();
            }

            var key = Mapper.GetRedisKey(entity.Id);
            var hashEntries = Mapper.MapToDestination(entity).ToArray();

            var tran = Database.CreateTransaction();
            tran.AddCondition(Condition.KeyExists(key));
            if (!string.IsNullOrEmpty(entryETag))
            {
                tran.AddCondition(Condition.HashEqual(key, Mapper.GetETagField(), entryETag));
            }

#pragma warning disable CS4014 // We can't await this one, as it is not "executed"; this is the way the API is built.
            tran.HashSetAsync(key, hashEntries);
#pragma warning restore CS4014

            var executed = await tran.ExecuteAsync().ConfigureAwait(false);
            if (!executed)
            {
                var exists = await Database.KeyExistsAsync(key).ConfigureAwait(false);
                if (!exists)
                {
                    var exception = new NotFoundException(typeof(TEntity).Name, entity.Id);
                    Logger.LogUpdatedWarning(
                        entityType: typeof(TEntity),
                        entityId: entity.Id,
                        warning: "NotFound",
                        exception: exception);

                    throw exception;
                }
                else
                {
                    var exception = new ConcurrencyException(typeof(TEntity).Name, entity.Id);
                    Logger.LogUpdatedWarning(
                        entityType: typeof(TEntity),
                        entityId: entity.Id,
                        warning: "Concurrency",
                        exception: exception);

                    throw exception;
                }
            }

            var result = Mapper.MapToSource(hashEntries);

            Logger.LogUpdated(
                entityType: typeof(TEntity),
                entityId: result.Id,
                entity: result);
            return result;
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

            var key = Mapper.GetRedisKey(id);
            var result = await Database.KeyDeleteAsync(key).ConfigureAwait(false);

            if (!result)
            {
                Logger.LogDeletedWarning(
                    entityType: typeof(TEntity),
                    entityId: id,
                    warning: "NotFound");
                return false;
            }

            Logger.LogDeleted(
                entityType: typeof(TEntity),
                entityId: id);

            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = Query.DefaultLimit, CancellationToken cancellationToken = default)
        {
            if (limit <= 0)
            {
                return ContinuationEnumerable.Empty<TEntity>();
            }

            var pagination = await ContinuationTokenEncoder.Decode(continuationToken);
            if (pagination is null)
            {
                pagination = new LimitOffsetContinuationToken { Limit = limit, Offset = 0 };
            }

            var allKeys = ServerForQuerying.Keys(pattern: Mapper.GetRedisKeyPattern()).ToList();
            var keys = allKeys
                .Skip(pagination.Offset)
                .Take(pagination.Limit)
                .ToList();

            var hashEntries = await Task.WhenAll(keys.Select(key => Database.HashGetAllAsync(key))).ConfigureAwait(false);

            string? nextEncodedToken = null;
            if ((hashEntries.Length + pagination.Offset) < allKeys.Count)
            {
                nextEncodedToken = await ContinuationTokenEncoder.Encode(
                    new LimitOffsetContinuationToken { Offset = pagination.Offset + pagination.Limit, Limit = pagination.Limit });
            }

            var result = hashEntries
                .Select(hashEntry => Mapper.MapToSource(hashEntry))
                .ToContinuationEnumerable(nextEncodedToken);

            Logger.LogFind($"(ct: {continuationToken ?? "<no-ct>"}, limit: {limit})", result);

            return result;
        }

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }
    }
}
