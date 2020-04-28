using Pantry.Continuation;
using Pantry.Queries;

namespace Pantry.Mediator.Repositories.Queries
{
    /// <summary>
    /// Standard domain query to find entities using a <see cref="ICriteriaRepositoryQuery{TResult}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public class FindByCriteriaDomainQuery<TEntity, TResult> : CriteriaRepositoryQuery<TEntity>, IDomainQuery<IContinuationEnumerable<TResult>>, IDomainRepositoryRequest
    {
    }
}
