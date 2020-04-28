using System;
using System.Threading;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using Pantry.Mediator.Repositories.Queries;
using Pantry.Traits;

namespace Pantry.Mediator.Repositories.Handlers
{
    /// <summary>
    /// Standard handler for <see cref="GetByIdDomainQuery{TEntity, TResult}"/> that use a <see cref="IRepositoryGet{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TQuery">The query type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public class GetByIdDomainQueryRepositoryHandler<TEntity, TQuery, TResult> : IDomainRequestHandler<TQuery, TResult>
        where TEntity : class, IIdentifiable
        where TQuery : GetByIdDomainQuery<TEntity, TResult>
    {
        private readonly IRepositoryGet<TEntity> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetByIdDomainQueryRepositoryHandler{TEntity, TRequest, TResult}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public GetByIdDomainQueryRepositoryHandler(IRepositoryGet<TEntity> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc/>
        public async Task<TResult> HandleAsync(TQuery request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var entityResult = await _repository.GetByIdAsync(request.Id).ConfigureAwait(false);
            return Mapper.Map<TEntity, TResult>(entityResult);
        }
    }
}
