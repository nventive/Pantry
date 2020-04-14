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
    /// <see cref="IDynamicTableEntityMapper{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class DynamicTableEntityMapper<TEntity> : IDynamicTableEntityMapper<TEntity>
        where TEntity : class, IIdentifiable, new()
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTableEntityMapper{T}"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public DynamicTableEntityMapper(ILogger<DynamicTableEntityMapper<TEntity>>? logger = null)
        {
            _logger = logger ?? NullLogger<DynamicTableEntityMapper<TEntity>>.Instance;
        }

        /// <inheritdoc/>
        public virtual DynamicTableEntity MapToDestination(TEntity source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var (partitionKey, rowKey) = GetStorageKeys(source.Id);
            var dynamicTableEntity = new DynamicTableEntity(partitionKey, rowKey);

            if (source is IETaggable taggableEntity)
            {
                dynamicTableEntity.ETag = taggableEntity.ETag;
            }

            if (source is ITimestamped timestampedEntity && timestampedEntity.Timestamp.HasValue)
            {
                dynamicTableEntity.Timestamp = timestampedEntity.Timestamp.Value;
            }

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
                Id = GetEntityId(destination.PartitionKey, destination.RowKey),
            };

            if (result is IETaggable taggableEntity)
            {
                taggableEntity.ETag = destination.ETag;
            }

            if (result is ITimestamped timestampedEntity)
            {
                timestampedEntity.Timestamp = destination.Timestamp;
            }

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

                        if ((property.PropertyType == typeof(DateTimeOffset) || property.PropertyType == typeof(DateTimeOffset?))
                            && dynamicValue is DateTime dateTimeValue)
                        {
                            dynamicValue = new DateTimeOffset(dateTimeValue);
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

        /// <inheritdoc/>
        public virtual (string partitionKey, string rowKey) GetStorageKeys(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return (id, id);
        }

        /// <inheritdoc/>
        public virtual string GetEntityId(string partitionKey, string rowKey) => rowKey;

        /// <inheritdoc/>
        public virtual IEnumerable<string> ResolveQueryPropertyPaths(string propertyPath)
            => propertyPath switch
            {
                "Id" => new[] { "PartitionKey", "RowKey" },
                _ => new[] { propertyPath },
            };

        /// <summary>
        /// Returns the list of serializable properties in <typeparamref name="TEntity"/>.
        /// </summary>
        /// <returns>The list of properties.</returns>
        protected IEnumerable<PropertyInfo> GetSerializableProperties()
        {
            return typeof(TEntity).GetProperties()
                .Where(x => x.CanRead && x.CanWrite)
                .Where(x => x.Name != nameof(IIdentifiable.Id) && x.Name != nameof(IETaggable.ETag) && x.Name != nameof(ITimestamped.Timestamp));
        }
    }
}
