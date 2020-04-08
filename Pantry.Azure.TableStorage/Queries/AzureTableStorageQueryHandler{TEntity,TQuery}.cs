﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Pantry.Continuation;
using Pantry.Mapping;
using Pantry.Queries;

namespace Pantry.Azure.TableStorage.Queries
{
    /// <summary>
    /// Base class for Azure Table Storage Query Handler implementations.
    /// <see cref="IAzureTableStorageQueryHandler{TEntity, TResult, TQuery}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The repository entity type.</typeparam>
    /// <typeparam name="TQuery">The query type.</typeparam>
    public abstract class AzureTableStorageQueryHandler<TEntity, TQuery> : IAzureTableStorageQueryHandler<TEntity, TEntity, TQuery>
        where TEntity : class, IIdentifiable
        where TQuery : IQuery<TEntity>
    {
        /// <inheritdoc />
        public virtual async Task<IContinuationEnumerable<TEntity>> Execute(
            IQuery<TEntity> query,
            CloudTableFor<TEntity> cloudTableFor,
            IMapper<TEntity, DynamicTableEntity> tableEntityMapper,
            CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (cloudTableFor is null)
            {
                throw new ArgumentNullException(nameof(cloudTableFor));
            }

            var tableQuery = new TableQuery<DynamicTableEntity>();
            tableQuery.Take(query.Limit);
            ApplyQueryToTableQuery(query, tableQuery);

            var operationResult = await cloudTableFor.CloudTable.ExecuteQuerySegmentedAsync(
                tableQuery,
                ContinuationToken.FromContinuationToken<TableContinuationToken>(query.ContinuationToken),
                cancellationToken).ConfigureAwait(false);

            var results = operationResult.Results
                .Select(x => tableEntityMapper.MapToSource(x))
                .ToContinuationEnumerable(ContinuationToken.ToContinuationToken(operationResult.ContinuationToken));

            return results;
        }

        /// <summary>
        /// Applies the specification from <paramref name="query"/> to <paramref name="tableQuery"/>.
        /// </summary>
        /// <param name="query">The original query.</param>
        /// <param name="tableQuery">The Table Entity query.</param>
        protected abstract void ApplyQueryToTableQuery(IQuery<TEntity> query, TableQuery<DynamicTableEntity> tableQuery);
    }
}
