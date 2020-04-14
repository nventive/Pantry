using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Continuation;
using Pantry.Queries;
using Pantry.Traits;

namespace Pantry.Decorators
{
    /// <summary>
    /// Base class for repository decorators.
    /// All methods act as pass-through so you can override just the ones you are interested in.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public abstract class RepositoryDecorator<TEntity> : IRepository<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryDecorator{TEntity}"/> class.
        /// </summary>
        /// <param name="innerRepository">The inner repository.</param>
        protected RepositoryDecorator(
            object innerRepository)
        {
            InnerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
        }

        /// <summary>
        /// Gets the Inner Repository.
        /// </summary>
        protected object InnerRepository { get; }

        /// <inheritdoc/>
        public virtual Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryAdd<TEntity>>().AddAsync(entity, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<(TEntity, bool)> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryAddOrUpdate<TEntity>>().AddOrUpdateAsync(entity, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = 50, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryFindAll<TEntity>>().FindAllAsync(continuationToken, limit, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaQuery<TEntity> query, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryFindByCriteria<TEntity>>().FindAsync(query, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<TEntity> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryGet<TEntity>>().GetByIdAsync(id, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryGet<TEntity>>().TryGetByIdAsync(id, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<IDictionary<string, TEntity?>> TryGetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryGet<TEntity>>().TryGetByIdsAsync(ids, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryGet<TEntity>>().ExistsAsync(id, cancellationToken);

        /// <inheritdoc/>
        public virtual Task RemoveAsync(string id, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryRemove<TEntity>>().RemoveAsync(id, cancellationToken);

        /// <inheritdoc/>
        public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryRemove<TEntity>>().RemoveAsync(entity, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<bool> TryRemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryRemove<TEntity>>().TryRemoveAsync(entity, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryRemove<TEntity>>().TryRemoveAsync(id, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => GetRepo<IRepositoryUpdate<TEntity>>().UpdateAsync(entity, cancellationToken);

        /// <summary>
        /// Gets the repository as <typeparamref name="TInterface"/>.
        /// </summary>
        /// <typeparam name="TInterface">The repostiory interface.</typeparam>
        /// <returns>The <typeparamref name="TInterface"/>.</returns>
        protected TInterface GetRepo<TInterface>() => (TInterface)InnerRepository;
    }
}
