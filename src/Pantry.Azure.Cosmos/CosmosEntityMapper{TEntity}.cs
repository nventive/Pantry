using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Pantry.Logging;
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
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosEntityMapper{TEntity}"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CosmosEntityMapper(
            ILogger<CosmosEntityMapper<TEntity>>? logger = null)
        {
            _logger = logger ?? NullLogger<CosmosEntityMapper<TEntity>>.Instance;
        }

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
                    document.Attributes[property.Name] = JToken.FromObject(value);
                    _logger.LogMapped(
                    entityType: typeof(CosmosDocument),
                    entityId: document.Id,
                    propertyName: property.Name,
                    propertyValue: document.Attributes[property.Name]);
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
                    var value = destination.Attributes[property.Name].ToObject(property.PropertyType);
                    property.SetValue(result, value);
                    _logger.LogMapped(
                        entityType: typeof(TEntity),
                        entityId: result.Id,
                        propertyName: property.Name,
                        propertyValue: value);
                }
            }

            return result;
        }
    }
}
