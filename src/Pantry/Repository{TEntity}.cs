using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Generators;
using Pantry.Providers;
using Pantry.Queries;

namespace Pantry
{
    /// <summary>
    /// Base class for <see cref="IRepository{TEntity}"/> implementations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public abstract class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <remarks>
        /// We chose to use <see cref="IServiceProvider"/> here a:
        /// - Repositories tend to have a large number of dependencies
        /// - Repositories are registered using extension methods, so this is mostly invisible
        /// - We want to be az lazy as possible
        /// - We want to avoid circular dependency resolution between repositories when possible.
        /// </remarks>
        protected Repository(
            IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected virtual IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the <see cref="IIdGenerator{T}"/>.
        /// </summary>
        protected virtual IIdGenerator<TEntity> IdGenerator => ServiceProvider.GetRequiredService<IIdGenerator<TEntity>>();

        /// <summary>
        /// Gets the <see cref="IETagGenerator{T}"/>.
        /// </summary>
        protected virtual IETagGenerator<TEntity> EtagGenerator => ServiceProvider.GetRequiredService<IETagGenerator<TEntity>>();

        /// <summary>
        /// Gets the <see cref="ITimestampProvider"/>.
        /// </summary>
        protected virtual ITimestampProvider TimestampProvider => ServiceProvider.GetRequiredService<ITimestampProvider>();

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected virtual ILogger Logger => ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType()) ?? NullLogger.Instance;

        /// <inheritdoc/>
        public virtual Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            throw new UnsupportedFeatureException($"This feature is not supported ({GetType().FullName}.{currentMethod.Name}).");
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
        public virtual Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = 50, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException($"This feature is not supported ({GetType().FullName}.{MethodBase.GetCurrentMethod().Name}).");
        }

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaRepositoryQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException($"This feature is not supported ({GetType().FullName}.{MethodBase.GetCurrentMethod().Name}).");
        }

        /// <inheritdoc/>
        public virtual Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException($"This feature is not supported ({GetType().FullName}.{MethodBase.GetCurrentMethod().Name}).");
        }

        /// <inheritdoc/>
        public virtual Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException($"This feature is not supported ({GetType().FullName}.{MethodBase.GetCurrentMethod().Name}).");
        }

        /// <inheritdoc/>
        public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException($"This feature is not supported ({GetType().FullName}.{MethodBase.GetCurrentMethod().Name}).");
        }
    }
}
