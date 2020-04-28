using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using Pantry.Continuation;
using Pantry.Mediator.Repositories.Queries;
using Pantry.Traits;

namespace Pantry.Mediator.Repositories.Handlers
{
    /// <summary>
    /// Standard handler for <see cref="FindByCriteriaDomainQuery{TEntity, TResult}"/> that use a <see cref="IRepositoryFindByCriteria{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TQuery">The query type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public class FindByCriteriaDomainQueryRepositoryHandler<TEntity, TQuery, TResult> : IDomainRequestHandler<TQuery, IContinuationEnumerable<TResult>>
        where TEntity : class, IIdentifiable
        where TQuery : FindByCriteriaDomainQuery<TEntity, TResult>
    {
        private readonly IRepositoryFindByCriteria<TEntity> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindByCriteriaDomainQueryRepositoryHandler{TEntity, TQuery, TResult}"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public FindByCriteriaDomainQueryRepositoryHandler(
            IRepositoryFindByCriteria<TEntity> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc/>
        public async Task<IContinuationEnumerable<TResult>> HandleAsync(TQuery request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var entityResult = await _repository.FindAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            return new ContinuationEnumerable<TResult>(
                entityResult.Select(x => Mapper.Map<TEntity, TResult>(x)),
                entityResult.ContinuationToken);
        }
    }
}
