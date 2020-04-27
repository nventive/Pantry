using System;
using System.Threading;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using Pantry.Mediator.Repositories.Commands;
using Pantry.Mediator.Repositories.DomainEvents;
using Pantry.Traits;

namespace Pantry.Mediator.Repositories.Handlers
{
    /// <summary>
    /// Standard handler for <see cref="CreateCommand{TEntity, TModel}"/> that use a <see cref="IRepositoryAdd{TEntity}"/>
    /// and pubish <see cref="EntityAddedDomainEvent{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public class CreateCommandRepositoryHandler<TEntity, TCommand, TResult> : IDomainRequestHandler<TCommand, TResult>
        where TEntity : class, IIdentifiable
        where TCommand : CreateCommand<TEntity, TResult>
    {
        private readonly IRepositoryAdd<TEntity> _repository;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCommandRepositoryHandler{TEntity, TRequest, TResult}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="mediator">The <see cref="IMediator"/>.</param>
        public CreateCommandRepositoryHandler(
            IRepositoryAdd<TEntity> repository,
            IMediator mediator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <inheritdoc/>
        public async Task<TResult> HandleAsync(TCommand request, CancellationToken cancellationToken)
        {
            var entity = Mapper.Map<TCommand, TEntity>(request);
            var entityResult = await _repository.AddAsync(entity).ConfigureAwait(false);
            await _mediator.PublishAsync(new EntityAddedDomainEvent<TEntity>(entityResult)).ConfigureAwait(false);
            return Mapper.Map<TEntity, TResult>(entityResult);
        }
    }
}
