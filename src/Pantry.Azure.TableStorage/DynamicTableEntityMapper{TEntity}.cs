using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Azure.Cosmos.Table;
using Pantry.Reflection;

namespace Pantry.Azure.TableStorage
{
    /// <summary>
    /// <see cref="IDynamicTableEntityMapper{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class DynamicTableEntityMapper<TEntity> : IDynamicTableEntityMapper<TEntity>
        where TEntity : class, IIdentifiable, new()
    {
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

            foreach (var property in EntityAttributes.GetAttributeProperties<TEntity>())
            {
                var value = property.GetValue(source);
                if (!DynamicTableEntityMapper.IsNativelySupportedAsProperty(property.PropertyType))
                {
                    value = JsonSerializer.Serialize(value);
                }

                dynamicTableEntity[property.Name] = EntityProperty.CreateEntityPropertyFromObject(value);
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

            foreach (var property in EntityAttributes.GetAttributeProperties<TEntity>())
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
                nameof(IIdentifiable.Id) => new[] { "PartitionKey", "RowKey" },
                _ => new[] { propertyPath },
            };
    }
}
