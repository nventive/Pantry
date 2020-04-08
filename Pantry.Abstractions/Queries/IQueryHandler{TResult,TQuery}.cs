using System.Threading;
using System.Threading.Tasks;
using Pantry.Continuation;

namespace Pantry.Queries
{
    /// <summary>
    /// Handles <typeparamref name="TQuery"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of results to return.</typeparam>
    /// <typeparam name="TQuery">The type of query.</typeparam>
    public interface IQueryHandler<TResult, TQuery>
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Executes the query and return the results.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The query execution results.</returns>
        Task<IContinuationEnumerable<TResult>> Execute(IQuery<TResult> query, CancellationToken cancellationToken = default);
    }
}
