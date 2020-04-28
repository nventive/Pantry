using Pantry.Continuation;
using Pantry.Queries;

namespace Pantry.Mediator.Repositories.Queries
{
    /// <summary>
    /// Standard domain query to find entities using a <see cref="ICriteriaRepositoryQuery{TResult}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class FindByCriteriaDomainQuery<TEntity> : FindByCriteriaDomainQuery<TEntity, TEntity>
    {
    }
}
