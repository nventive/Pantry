using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Pantry.Continuation;
using Pantry.Mapping;
using Pantry.Queries;

namespace Pantry.Azure.TableStorage.Queries
{
    /// <summary>
    /// Handler for queries destined for <see cref="AzureTableStorageRepository{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The repository entity type.</typeparam>
    /// <typeparam name="TResult">The query result type.</typeparam>
    /// <typeparam name="TQuery">The query type.</typeparam>
    public interface IAzureTableStorageQueryHandler<TEntity, TResult, TQuery> : IAzureTableStorageQueryHandler
        where TEntity : class, IIdentifiable
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Executes the query and return the results.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cloudTableFor">The <see cref="CloudTableFor{T}"/>.</param>
        /// <param name="tableEntityMapper">The <see cref="IMapper{TSource, TDestination}"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The query execution results.</returns>
        Task<IContinuationEnumerable<TResult>> Execute(
            IQuery<TResult> query,
            CloudTableFor<TEntity> cloudTableFor,
            IMapper<TEntity, DynamicTableEntity> tableEntityMapper,
            CancellationToken cancellationToken = default);
    }
}
