using System;
using System.Threading;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using Pantry.Mediator.Repositories.Commands;
using Pantry.Traits;

namespace Pantry.Mediator.Repositories.Handlers
{
    /// <summary>
    /// Standard handler for <see cref="CreateCommand{TEntity, TModel}"/> that use a <see cref="IRepositoryAdd{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public class CreateCommandRepositoryHandler<TEntity, TCommand, TResult> : IDomainRequestHandler<TCommand, TResult>
        where TEntity : class, IIdentifiable
        where TCommand : CreateCommand<TEntity, TResult>
    {
        private readonly IRepositoryAdd<TEntity> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCommandRepositoryHandler{TEntity, TRequest, TResult}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public CreateCommandRepositoryHandler(IRepositoryAdd<TEntity> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc/>
        public async Task<TResult> HandleAsync(TCommand request, CancellationToken cancellationToken)
        {
            var entity = Mapper.Map<TCommand, TEntity>(request);
            var entityResult = await _repository.AddAsync(entity).ConfigureAwait(false);
            return Mapper.Map<TEntity, TResult>(entityResult);
        }
    }
}
