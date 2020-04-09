using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Logging;
using Pantry.Mapping;

namespace Pantry.Azure.TableStorage
{
    /// <summary>
    /// Maps entities to and from <see cref="DynamicTableEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class DynamicTableEntityMapper<TEntity> : IMapper<TEntity, DynamicTableEntity>
        where TEntity : class, IIdentifiable, new()
    {
        private readonly IAzureTableStorageKeysResolver<TEntity> _keysResolver;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTableEntityMapper{T}"/> class.
        /// </summary>
        /// <param name="keysResolver">The <see cref="IAzureTableStorageKeysResolver{T}"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public DynamicTableEntityMapper(
            IAzureTableStorageKeysResolver<TEntity> keysResolver,
            ILogger<DynamicTableEntityMapper<TEntity>>? logger = null)
        {
            _keysResolver = keysResolver ?? throw new ArgumentNullException(nameof(keysResolver));
            _logger = logger ?? NullLogger<DynamicTableEntityMapper<TEntity>>.Instance;
        }

        /// <inheritdoc/>
        public virtual DynamicTableEntity MapToDestination(TEntity source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var (partitionKey, rowKey) = _keysResolver.GetStorageKeys(source.Id);
            var dynamicTableEntity = new DynamicTableEntity(partitionKey, rowKey);

            foreach (var property in GetSerializableProperties())
            {
                var value = property.GetValue(source);
                if (!DynamicTableEntityMapper.IsNativelySupportedAsProperty(property.PropertyType))
                {
                    value = JsonSerializer.Serialize(value);
                }

                dynamicTableEntity[property.Name] = EntityProperty.CreateEntityPropertyFromObject(value);
                _logger.LogMapped(
                    entityType: typeof(DynamicTableEntity),
                    entityId: dynamicTableEntity.RowKey,
                    propertyName: property.Name,
                    propertyValue: value);
            }

            return dynamicTableEntity;
        }

        /// <inheritdoc/>
        public virtual TEntity MapToSource(DynamicTableEntity destination)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            var result = new TEntity
            {
                Id = _keysResolver.GetEntityId(destination.PartitionKey, destination.RowKey),
            };

            foreach (var property in GetSerializableProperties())
            {
                if (destination.Properties.ContainsKey(property.Name))
                {
                    var dynamicValue = destination[property.Name].PropertyAsObject;
                    if (dynamicValue != null)
                    {
                        if (!DynamicTableEntityMapper.IsNativelySupportedAsProperty(property.PropertyType))
                        {
                            dynamicValue = JsonSerializer.Deserialize(destination[property.Name].StringValue, property.PropertyType);
                        }

                        property.SetValue(result, dynamicValue);
                        _logger.LogMapped(
                            entityType: typeof(DynamicTableEntity),
                            entityId: result.Id,
                            propertyName: property.Name,
                            propertyValue: dynamicValue);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the list of serializable properties in <typeparamref name="TEntity"/>.
        /// </summary>
        /// <returns>The list of properties.</returns>
        protected IEnumerable<PropertyInfo> GetSerializableProperties()
        {
            return typeof(TEntity).GetProperties()
                .Where(x => x.CanRead && x.CanWrite)
                .Where(x => x.Name != nameof(IIdentifiable.Id));
        }
    }
}
