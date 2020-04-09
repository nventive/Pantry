using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Mapping;
using Pantry.Queries;

namespace Pantry.Azure.TableStorage.Queries
{
    /// <summary>
    /// Executes <see cref="MirrorQuery{TResult}"/> for Azure Table Storage.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class AzureTableStorageMirrorQueryHandler<TEntity> : AzureTableStorageQueryHandler<TEntity, MirrorQuery<TEntity>>
        where TEntity : class, IIdentifiable, new()
    {
        private readonly IAzureTableStorageKeysResolver<TEntity> _keysResolver;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageMirrorQueryHandler{TEntity}"/> class.
        /// </summary>
        /// <param name="cloudTableFor">The <see cref="CloudTableFor{T}"/>.</param>
        /// <param name="tableEntityMapper">The <see cref="IMapper{TSource, TDestination}"/>.</param>
        /// <param name="keysResolver">The <see cref="IAzureTableStorageKeysResolver{T}"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public AzureTableStorageMirrorQueryHandler(
            CloudTableFor<TEntity> cloudTableFor,
            IMapper<TEntity, DynamicTableEntity> tableEntityMapper,
            IAzureTableStorageKeysResolver<TEntity> keysResolver,
            ILogger<AzureTableStorageMirrorQueryHandler<TEntity>>? logger = null)
            : base(cloudTableFor, tableEntityMapper)
        {
            _keysResolver = keysResolver ?? throw new ArgumentNullException(nameof(keysResolver));
            _logger = logger ?? NullLogger<AzureTableStorageMirrorQueryHandler<TEntity>>.Instance;
        }

        /// <inheritdoc/>
        protected override void ApplyQueryToTableQuery(MirrorQuery<TEntity> query, TableQuery<DynamicTableEntity> tableQuery)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (tableQuery is null)
            {
                throw new ArgumentNullException(nameof(tableQuery));
            }

            if (query.Mirror is null)
            {
                return;
            }

            var filters = new List<string>();

            if (!string.IsNullOrEmpty(query.Mirror.Id))
            {
                var (partitionKey, rowKey) = _keysResolver.GetStorageKeys(query.Mirror.Id);

                if (!string.IsNullOrEmpty(partitionKey))
                {
                    filters.Add(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
                }

                if (!string.IsNullOrEmpty(partitionKey))
                {
                    filters.Add(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));
                }
            }

            var possibleFilterProperties = typeof(TEntity).GetProperties()
                .Where(x => x.CanRead && x.CanWrite)
                .Where(x => x.Name != nameof(IIdentifiable.Id))
                .Where(x => DynamicTableEntityMapper.IsNativelySupportedAsProperty(x.PropertyType));

            foreach (var property in possibleFilterProperties)
            {
                var value = property.GetValue(query.Mirror);
                if (value != null)
                {
                    var defaultValueType = property.PropertyType == typeof(string) ? null : Activator.CreateInstance(property.PropertyType);
                    if (value != defaultValueType)
                    {
                        filters.Add(TableQuery.GenerateFilterCondition(property.Name, QueryComparisons.Equal, $"{value}"));
                    }
                }
            }

            if (filters.Any())
            {
                tableQuery.FilterString = string.Join(" AND ", filters.Select(x => $"({x})"));
            }

            _logger.LogTrace("ApplyQueryToTableQuery(): {FilterString}", tableQuery.FilterString);
        }
    }
}
