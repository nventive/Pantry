using Microsoft.Azure.Cosmos.Table;
using Pantry.Queries;

namespace Pantry.Azure.TableStorage.Queries
{
    /// <summary>
    /// Executes <see cref="AllQuery{TResult}"/> for Azure Table Storage.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class AzureTableStorageAllQueryHandler<T> : AzureTableStorageQueryHandler<T, AllQuery<T>>
        where T : class, IIdentifiable
    {
        /// <inheritdoc/>
        protected override void ApplyQueryToTableQuery(IQuery<T> query, TableQuery<DynamicTableEntity> tableQuery)
        {
            // No-op, we select all.
        }
    }
}
