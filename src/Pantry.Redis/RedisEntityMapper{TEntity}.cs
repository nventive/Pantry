using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Pantry.Reflection;
using StackExchange.Redis;

namespace Pantry.Redis
{
    /// <summary>
    /// <see cref="IRedisEntityMapper{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class RedisEntityMapper<TEntity> : IRedisEntityMapper<TEntity>
        where TEntity : class, IIdentifiable, new()
    {
        /// <inheritdoc/>
        public RedisKey GetRedisKey(string id)
        {
            return new RedisKey($"{typeof(TEntity).Name}:{id}");
        }

        /// <inheritdoc/>
        public IEnumerable<HashEntry> MapToDestination(TEntity source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            yield return new HashEntry(nameof(IIdentifiable.Id), source.Id);

            if (source is IETaggable taggableEntity)
            {
                yield return new HashEntry(nameof(IETaggable.ETag), taggableEntity.ETag);
            }

            if (source is ITimestamped timestampedEntity && timestampedEntity.Timestamp.HasValue)
            {
                yield return new HashEntry(nameof(ITimestamped.Timestamp), timestampedEntity.Timestamp.Value.ToUnixTimeMilliseconds());
            }

            foreach (var property in EntityAttributes.GetAttributeProperties<TEntity>())
            {
                var value = property.GetValue(source);
                if (!RedisEntityMapper.IsNativelySupportedAsProperty(property.PropertyType))
                {
                    value = JsonSerializer.Serialize(value);
                }

                yield return new HashEntry(property.Name, value.ToString());
            }
        }

        /// <inheritdoc/>
        public TEntity MapToSource(IEnumerable<HashEntry> destination)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            var allDestinationValues = destination.ToDictionary(x => x.Name.ToString());

            var result = new TEntity
            {
                Id = allDestinationValues[nameof(IIdentifiable.Id)].Value.ToString(),
            };

            if (result is IETaggable taggableEntity)
            {
                taggableEntity.ETag = allDestinationValues[nameof(IETaggable.ETag)].Value.ToString();
            }

            if (result is ITimestamped timestampedEntity && allDestinationValues.ContainsKey(nameof(ITimestamped.Timestamp)))
            {
                timestampedEntity.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(
                    (long)allDestinationValues[nameof(ITimestamped.Timestamp)].Value);
            }

            foreach (var property in EntityAttributes.GetAttributeProperties<TEntity>())
            {
                if (allDestinationValues.ContainsKey(property.Name))
                {
                    var redisValue = allDestinationValues[property.Name].Value;
                    if (!RedisEntityMapper.IsNativelySupportedAsProperty(property.PropertyType))
                    {
                        property.SetValue(result, JsonSerializer.Deserialize(allDestinationValues[property.Name].Value.ToString(), property.PropertyType));
                    }
                    else
                    {
                        property.SetValueWithCastOperators(result, redisValue);
                    }
                }
            }

            return result;
        }
    }
}
