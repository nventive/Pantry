using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Decorators;

namespace Pantry.DomainEvents
{
    /// <summary>
    /// <see cref="IRepository{TEntity}"/> decorator that sends <see cref="IDomainEvent"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "This is a decorator.")]
    public class DomainEventRepositoryDecorator<TEntity> : RepositoryDecorator<TEntity>
        where TEntity : class, IIdentifiable
    {
        private readonly IDomainEventsDispatcher _dispatcher;
        private readonly ILogger<DomainEventRepositoryDecorator<TEntity>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventRepositoryDecorator{TEntity}"/> class.
        /// </summary>
        /// <param name="dispatcher">The <see cref="IDomainEventsDispatcher"/>.</param>
        /// <param name="innerRepository">The inner repository.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public DomainEventRepositoryDecorator(
            IDomainEventsDispatcher dispatcher,
            object innerRepository,
            ILogger<DomainEventRepositoryDecorator<TEntity>>? logger = null)
            : base(innerRepository)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _logger = logger ?? NullLogger<DomainEventRepositoryDecorator<TEntity>>.Instance;
        }

        /// <inheritdoc/>
        public override async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = await base.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = new EntityAddedDomainEvent<TEntity> { Entity = result };
            _logger.LogTrace("Dispatching {DomainEvent}", domainEvent);
            await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public override async Task<(TEntity, bool)> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var (result, added) = await base.AddOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = added
                ? new EntityAddedDomainEvent<TEntity> { Entity = result }
                : (IDomainEvent)new EntityUpdatedDomainEvent<TEntity> { Entity = result };
            _logger.LogTrace("Dispatching {DomainEvent}", domainEvent);
            await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
            return (result, added);
        }

        /// <inheritdoc/>
        public override async Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await base.TryRemoveAsync(id, cancellationToken).ConfigureAwait(false);
            if (result)
            {
                var domainEvent = new EntityRemovedDomainEvent<TEntity> { EntityId = id };
                _logger.LogTrace("Dispatching {DomainEvent}", domainEvent);
                await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
            }

            return result;
        }

        /// <inheritdoc/>
        public override async Task<bool> TryRemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = await base.TryRemoveAsync(entity, cancellationToken).ConfigureAwait(false);
            if (result)
            {
                var domainEvent = new EntityRemovedDomainEvent<TEntity> { EntityId = entity.Id };
                _logger.LogTrace("Dispatching {DomainEvent}", domainEvent);
                await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
            }

            return result;
        }

        /// <inheritdoc/>
        public override async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            await base.RemoveAsync(id, cancellationToken).ConfigureAwait(false);
            var domainEvent = new EntityRemovedDomainEvent<TEntity> { EntityId = id };
            _logger.LogTrace("Dispatching {DomainEvent}", domainEvent);
            await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await base.RemoveAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = new EntityRemovedDomainEvent<TEntity> { EntityId = entity.Id };
            _logger.LogTrace("Dispatching {DomainEvent}", domainEvent);
            await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = await base.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = new EntityUpdatedDomainEvent<TEntity> { Entity = result };
            _logger.LogTrace("Dispatching {DomainEvent}", domainEvent);
            await _dispatcher.DispatchAsync(domainEvent).ConfigureAwait(false);
            return result;
        }
    }
}
