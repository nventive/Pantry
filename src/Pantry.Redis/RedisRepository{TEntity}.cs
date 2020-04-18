using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Generators;
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
        /// <param name="timestampProvider">The <see cref="ITimestampProvider"/>.</param>
        /// <param name="mapper">The <see cref="IRedisEntityMapper{TEntity}"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public RedisRepository(
            RedisDatabaseFor<TEntity> databaseFor,
            IIdGenerator<TEntity> idGenerator,
            ITimestampProvider timestampProvider,
            IRedisEntityMapper<TEntity> mapper,
            ILogger<RedisRepository<TEntity>>? logger = null)
        {
            DatabaseFor = databaseFor ?? throw new ArgumentNullException(nameof(databaseFor));
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            TimestampProvider = timestampProvider ?? throw new ArgumentNullException(nameof(timestampProvider));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
        /// Gets the <see cref="ITimestampProvider"/>.
        /// </summary>
        protected ITimestampProvider TimestampProvider { get; }

        /// <summary>
        /// Gets the <see cref="IRedisEntityMapper{TEntity}"/>.
        /// </summary>
        protected IRedisEntityMapper<TEntity> Mapper { get; }

        /// <summary>
        /// Gets the <see cref="IDatabase"/>.
        /// </summary>
        protected IDatabaseAsync Database => DatabaseFor.Database;

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc/>
        public virtual Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
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
        public virtual Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = 50, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }
    }
}
