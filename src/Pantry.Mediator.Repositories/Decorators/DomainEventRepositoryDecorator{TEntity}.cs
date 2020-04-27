using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Decorators;
using Pantry.Mediator.Repositories.DomainEvents;

namespace Pantry.Mediator.Repositories.Decorators
{
    /// <summary>
    /// <see cref="IRepository{TEntity}"/> decorator that sends <see cref="IDomainEvent"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "This is a decorator.")]
    public class DomainEventRepositoryDecorator<TEntity> : RepositoryDecorator<TEntity>
        where TEntity : class, IIdentifiable
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventRepositoryDecorator{TEntity}"/> class.
        /// </summary>
        /// <param name="mediator">The <see cref="IMediator"/>.</param>
        /// <param name="innerRepository">The inner repository.</param>
        public DomainEventRepositoryDecorator(
            IMediator mediator,
            object innerRepository)
            : base(innerRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <inheritdoc/>
        public override async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = await base.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = new EntityAddedDomainEvent<TEntity>(result);
            await _mediator.PublishAsync(domainEvent).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public override async Task<(TEntity, bool)> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var (result, added) = await base.AddOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = added ? new EntityAddedDomainEvent<TEntity>(result) : (IDomainEvent)new EntityUpdatedDomainEvent<TEntity>(result);
            await _mediator.PublishAsync(domainEvent).ConfigureAwait(false);
            return (result, added);
        }

        /// <inheritdoc/>
        public override async Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await base.TryRemoveAsync(id, cancellationToken).ConfigureAwait(false);
            if (result)
            {
                var domainEvent = new EntityRemovedDomainEvent<TEntity>(id);
                await _mediator.PublishAsync(domainEvent).ConfigureAwait(false);
            }

            return result;
        }

        /// <inheritdoc/>
        public override async Task<bool> TryRemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = await base.TryRemoveAsync(entity, cancellationToken).ConfigureAwait(false);
            if (result)
            {
                var domainEvent = new EntityRemovedDomainEvent<TEntity>(entity.Id);
                await _mediator.PublishAsync(domainEvent).ConfigureAwait(false);
            }

            return result;
        }

        /// <inheritdoc/>
        public override async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            await base.RemoveAsync(id, cancellationToken).ConfigureAwait(false);
            var domainEvent = new EntityRemovedDomainEvent<TEntity>(id);
            await _mediator.PublishAsync(domainEvent).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await base.RemoveAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = new EntityRemovedDomainEvent<TEntity>(entity.Id);
            await _mediator.PublishAsync(domainEvent).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var result = await base.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            var domainEvent = new EntityUpdatedDomainEvent<TEntity>(result);
            await _mediator.PublishAsync(domainEvent).ConfigureAwait(false);
            return result;
        }
    }
}
