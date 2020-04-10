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
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageQueryHandler{TEntity, TQuery}"/> class.
        /// </summary>
        /// <param name="cloudTableFor">The <see cref="CloudTableFor{T}"/>.</param>
        /// <param name="tableEntityMapper">The <see cref="IMapper{TSource, TDestination}"/>.</param>
        /// <param name="tokenEncoder">The continuation token encoder.</param>
        protected AzureTableStorageQueryHandler(
            CloudTableFor<TEntity> cloudTableFor,
            IMapper<TEntity, DynamicTableEntity> tableEntityMapper,
            IContinuationTokenEncoder<TableContinuationToken> tokenEncoder)
        {
            CloudTableFor = cloudTableFor ?? throw new ArgumentNullException(nameof(cloudTableFor));
            TableEntityMapper = tableEntityMapper ?? throw new ArgumentNullException(nameof(tableEntityMapper));
            TokenEncoder = tokenEncoder ?? throw new ArgumentNullException(nameof(tokenEncoder));
        }

        /// <summary>
        /// Gets the <see cref="CloudTableFor{T}"/>.
        /// </summary>
        protected CloudTableFor<TEntity> CloudTableFor { get; }

        /// <summary>
        /// Gets the <see cref="IMapper{TSource, TDestination}"/>.
        /// </summary>
        protected IMapper<TEntity, DynamicTableEntity> TableEntityMapper { get; }

        /// <summary>
        /// Gets the <see cref="IContinuationTokenEncoder{TableContinuationToken}"/>.
        /// </summary>
        protected IContinuationTokenEncoder<TableContinuationToken> TokenEncoder { get; }

        /// <inheritdoc />
        public virtual async Task<IContinuationEnumerable<TEntity>> ExecuteAsync(
            TQuery query,
            CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var tableQuery = new TableQuery<DynamicTableEntity>();
            tableQuery.Take(query.Limit);
            ApplyQueryToTableQuery(query, tableQuery);

            var operationResult = await CloudTableFor.CloudTable.ExecuteQuerySegmentedAsync(
                tableQuery,
                await TokenEncoder.Decode(query.ContinuationToken),
                cancellationToken).ConfigureAwait(false);

            var results = operationResult.Results
                .Select(x => TableEntityMapper.MapToSource(x))
                .ToContinuationEnumerable(await TokenEncoder.Encode(operationResult.ContinuationToken));

            return results;
        }

        /// <summary>
        /// Applies the specification from <paramref name="query"/> to <paramref name="tableQuery"/>.
        /// </summary>
        /// <param name="query">The original query.</param>
        /// <param name="tableQuery">The Table Entity query.</param>
        protected abstract void ApplyQueryToTableQuery(TQuery query, TableQuery<DynamicTableEntity> tableQuery);
    }
}
