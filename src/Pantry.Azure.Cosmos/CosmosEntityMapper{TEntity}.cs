using System;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Pantry.Reflection;

namespace Pantry.Azure.Cosmos
{
    /// <summary>
    /// <see cref="ICosmosEntityMapper{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class CosmosEntityMapper<TEntity> : ICosmosEntityMapper<TEntity>
        where TEntity : class, IIdentifiable, new()
    {
        /// <inheritdoc/>
        public virtual PartitionKey GetPartitionKey(string id) => new PartitionKey(id);

        /// <inheritdoc/>
        public virtual string GetEntityType() => typeof(TEntity).Name;

        /// <inheritdoc/>
        public virtual string ResolveQueryPropertyPaths(string propertyPath)
            => propertyPath switch
            {
                nameof(IIdentifiable.Id) => "id",
                nameof(ITimestamped.Timestamp) => "_ts",
                _ => propertyPath,
            };

        /// <inheritdoc/>
        public virtual CosmosDocument MapToDestination(TEntity source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var document = new CosmosDocument
            {
                EntityType = GetEntityType(),
                Id = source.Id,
            };

            foreach (var property in EntityAttributes.GetAttributeProperties<TEntity>())
            {
                var value = property.GetValue(source);
                if (value != null)
                {
                    if (value is DateTimeOffset dto)
                    {
                        value = dto.ToUnixTimeMilliseconds();
                    }

                    document.Attributes[property.Name] = JToken.FromObject(value);
                }
            }

            return document;
        }

        /// <inheritdoc/>
        public virtual TEntity MapToSource(CosmosDocument destination)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            var result = new TEntity
            {
                Id = destination.Id,
            };

            if (result is IETaggable taggable
                && destination.Attributes.ContainsKey(CosmosDocument.SystemETagAttribute)
                && destination.Attributes[CosmosDocument.SystemETagAttribute] != null)
            {
                taggable.ETag = destination.Attributes[CosmosDocument.SystemETagAttribute].ToObject<string>();
            }

            if (result is ITimestamped timestampedEntity
                && destination.Attributes.ContainsKey(CosmosDocument.SystemTimestampAttribute)
                && destination.Attributes[CosmosDocument.SystemTimestampAttribute] != null)
            {
                timestampedEntity.Timestamp = DateTimeOffset.FromUnixTimeSeconds(
                    destination.Attributes[CosmosDocument.SystemTimestampAttribute].ToObject<long>());
            }

            foreach (var property in EntityAttributes.GetAttributeProperties<TEntity>())
            {
                if (destination.Attributes.ContainsKey(property.Name) && destination.Attributes[property.Name] != null)
                {
                    object value;
                    if (property.PropertyType == typeof(DateTimeOffset) || property.PropertyType == typeof(DateTimeOffset?))
                    {
                        value = DateTimeOffset.FromUnixTimeMilliseconds(destination.Attributes[property.Name].ToObject<long>());
                    }
                    else
                    {
                        value = destination.Attributes[property.Name].ToObject(property.PropertyType);
                    }

                    property.SetValue(result, value);
                }
            }

            return result;
        }
    }
}
