using System;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Mediator.Repositories.Commands;
using Pantry.Traits;

namespace Pantry.Mediator.Repositories.Handlers
{
    /// <summary>
    /// Standard handler for <see cref="DeleteCommand{TEntity}"/> that use a <see cref="IRepositoryRemove{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TCommand">The command type.</typeparam>
    public class DeleteCommandRepositoryHandler<TEntity, TCommand> : IDomainRequestHandler<TCommand, bool>
        where TEntity : class, IIdentifiable
        where TCommand : DeleteCommand<TEntity>
    {
        private readonly IRepositoryRemove<TEntity> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCommandRepositoryHandler{TEntity, TCommand}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public DeleteCommandRepositoryHandler(IRepositoryRemove<TEntity> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc/>
        public async Task<bool> HandleAsync(TCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return await _repository.TryRemoveAsync(request.Id, cancellationToken).ConfigureAwait(false);
        }
    }
}
