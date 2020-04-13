using System;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Continuation;
using Pantry.Queries;
using Pantry.Traits;

namespace Pantry.DomainEvents
{
    /// <summary>
    /// <see cref="IRepository{TEntity}"/> decorator that sends <see cref="IDomainEvent"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class DomainEventRepositoryDecorator<TEntity> : IRepository<TEntity>
        where TEntity : class, IIdentifiable
    {
        private readonly IDomainEventsDispatcher _dispatcher;
        private readonly object _innerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventRepositoryDecorator{TEntity}"/> class.
        /// </summary>
        /// <param name="dispatcher">The <see cref="IDomainEventsDispatcher"/>.</param>
        /// <param name="innerRepository">The inner repository.</param>
        public DomainEventRepositoryDecorator(
            IDomainEventsDispatcher dispatcher,
            object innerRepository)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _innerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
        }

        /// <inheritdoc/>
        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = await ((IRepositoryAdd<TEntity>)_innerRepository).AddAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = new EntityAddedDomainEvent<TEntity> { Entity = result };
            await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<(TEntity, bool)> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var (result, added) = await ((IRepositoryAddOrUpdate<TEntity>)_innerRepository).AddOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = added
                ? new EntityAddedDomainEvent<TEntity> { Entity = result }
                : (IDomainEvent)new EntityUpdatedDomainEvent<TEntity> { Entity = result };
            await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
            return (result, added);
        }

        /// <inheritdoc/>
        public async Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await ((IRepositoryRemove<TEntity>)_innerRepository).TryRemoveAsync(id, cancellationToken).ConfigureAwait(false);
            if (result)
            {
                var domainEvent = new EntityRemovedDomainEvent<TEntity> { EntityId = id };
                await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = await ((IRepositoryUpdate<TEntity>)_innerRepository).UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = new EntityUpdatedDomainEvent<TEntity> { Entity = result };
            await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = 50, CancellationToken cancellationToken = default)
            => ((IRepositoryFindAll<TEntity>)_innerRepository).FindAllAsync(continuationToken, limit, cancellationToken);

        /// <inheritdoc/>
        public Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaQuery<TEntity> query, CancellationToken cancellationToken = default)
            => ((IRepositoryFindByCriteria<TEntity>)_innerRepository).FindAsync(query, cancellationToken);

        /// <inheritdoc/>
        public Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
            => ((IRepositoryGet<TEntity>)_innerRepository).TryGetByIdAsync(id, cancellationToken);
    }
}
