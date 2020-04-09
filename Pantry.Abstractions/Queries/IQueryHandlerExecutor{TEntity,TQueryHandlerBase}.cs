using System.Threading;
using System.Threading.Tasks;
using Pantry.Continuation;

namespace Pantry.Queries
{
    /// <summary>
    /// Executes query handlers.
    /// </summary>
    /// <typeparam name="TEntity">The entity type. </typeparam>
    /// <typeparam name="TQueryHandlerBase">The query handler base type used to identify the factory.</typeparam>
    public interface IQueryHandlerExecutor<TEntity, TQueryHandlerBase>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Executes the query and return the results.
        /// </summary>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The query execution results.</returns>
        Task<IContinuationEnumerable<TResult>> ExecuteAsync<TResult, TQuery>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResult>;
    }
}
