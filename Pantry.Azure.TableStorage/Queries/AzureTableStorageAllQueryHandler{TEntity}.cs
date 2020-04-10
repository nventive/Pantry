using Microsoft.Azure.Cosmos.Table;
using Pantry.Continuation;
using Pantry.Mapping;
using Pantry.Queries;

namespace Pantry.Azure.TableStorage.Queries
{
    /// <summary>
    /// Executes <see cref="AllQuery{TResult}"/> for Azure Table Storage.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class AzureTableStorageAllQueryHandler<TEntity> : AzureTableStorageQueryHandler<TEntity, AllQuery<TEntity>>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageAllQueryHandler{TEntity}"/> class.
        /// </summary>
        /// <param name="cloudTableFor">The <see cref="CloudTableFor{T}"/>.</param>
        /// <param name="tableEntityMapper">The <see cref="IMapper{TSource, TDestination}"/>.</param>
        /// <param name="tokenEncoder">The continuation token encoder.</param>
        public AzureTableStorageAllQueryHandler(
            CloudTableFor<TEntity> cloudTableFor,
            IMapper<TEntity, DynamicTableEntity> tableEntityMapper,
            IContinuationTokenEncoder<TableContinuationToken> tokenEncoder)
            : base(cloudTableFor, tableEntityMapper, tokenEncoder)
        {
        }

        /// <inheritdoc/>
        protected override void ApplyQueryToTableQuery(AllQuery<TEntity> query, TableQuery<DynamicTableEntity> tableQuery)
        {
            // No-op, we select all.
        }
    }
}
