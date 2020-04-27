using Microsoft.Azure.Cosmos.Table;
using Pantry.Queries;

namespace Pantry.Azure.TableStorage.Queries
{
    /// <summary>
    /// Base class for Azure Table Storage that uses <see cref="TableQuery"/>.
    /// </summary>
    /// <typeparam name="TResult">The query result type.</typeparam>
    public abstract class AzureTableStorageTableQuery<TResult> : RepositoryQuery<TResult>
    {
        /// <summary>
        /// Apply the query to the <paramref name="tableQuery"/>.
        /// </summary>
        /// <param name="tableQuery">The <see cref="TableQuery"/>.</param>
        public abstract void Apply(TableQuery<DynamicTableEntity> tableQuery);
    }
}
