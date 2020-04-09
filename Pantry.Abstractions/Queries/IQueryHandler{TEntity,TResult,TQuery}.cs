using System.Threading;
using System.Threading.Tasks;
using Pantry.Continuation;

namespace Pantry.Queries
{
    /// <summary>
    /// Handles query execution.
    /// </summary>
    /// <typeparam name="TEntity">The entity repository type.</typeparam>
    /// <typeparam name="TResult">The query result type.</typeparam>
    /// <typeparam name="TQuery">The query type.</typeparam>
    public interface IQueryHandler<TEntity, TResult, TQuery>
        where TEntity : class, IIdentifiable
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Executes the query and return the results.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The query execution results.</returns>
        Task<IContinuationEnumerable<TResult>> ExecuteAsync(
            TQuery query,
            CancellationToken cancellationToken = default);
    }
}
