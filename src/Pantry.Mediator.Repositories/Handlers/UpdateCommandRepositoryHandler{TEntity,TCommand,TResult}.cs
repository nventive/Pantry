using System;
using System.Threading;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using Pantry.Mediator.Repositories.Commands;

namespace Pantry.Mediator.Repositories.Handlers
{
    /// <summary>
    /// Standard handler for <see cref="UpdateCommand{TEntity, TModel}"/> that use a <see cref="ICrudRepository{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public class UpdateCommandRepositoryHandler<TEntity, TCommand, TResult> : IDomainRequestHandler<TCommand, TResult>
        where TEntity : class, IIdentifiable
        where TCommand : UpdateCommand<TEntity, TResult>
    {
        private readonly ICrudRepository<TEntity> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCommandRepositoryHandler{TEntity, TRequest, TResult}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public UpdateCommandRepositoryHandler(ICrudRepository<TEntity> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc/>
        public async Task<TResult> HandleAsync(TCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            entity.InjectFrom(request);
            var entityResult = await _repository.UpdateAsync(entity, cancellationToken: cancellationToken).ConfigureAwait(false);
            return Mapper.Map<TEntity, TResult>(entityResult);
        }
    }
}
