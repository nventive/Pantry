using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using Pantry.Reflection;

namespace Pantry.PetaPoco
{
    /// <summary>
    /// <see cref="IPetaPocoEntityMapper{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class PetaPocoEntityMapper<TEntity> : IPetaPocoEntityMapper<TEntity>
        where TEntity : class, IIdentifiable, new()
    {
        /// <inheritdoc/>
        public object GetPrimaryKey(string id) => id;

        /// <inheritdoc/>
        public string GetTableName() => typeof(TEntity).Name;

        /// <inheritdoc/>
        public string GetPrimaryKeyName() => nameof(IIdentifiable.Id);

        /// <inheritdoc/>
        public ExpandoObject MapToDestination(TEntity source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            dynamic destination = new ExpandoObject();
            destination.Id = source.Id;

            if (source is IETaggable taggableEntity && !string.IsNullOrEmpty(taggableEntity.ETag))
            {
                destination.ETag = taggableEntity.ETag;
            }

            if (source is ITimestamped timestampedEntity && timestampedEntity.Timestamp.HasValue)
            {
                destination.Timestamp = timestampedEntity.Timestamp.Value;
            }

            foreach (var property in EntityAttributes.GetAttributeProperties<TEntity>())
            {
                var value = property.GetValue(source);
                if (value != null)
                {
                    if (!PetaPocoEntityMapper.IsNativelySupportedAsProperty(property.PropertyType))
                    {
                        value = JsonSerializer.Serialize(value);
                    }

                    ((IDictionary<string, object>)destination)[property.Name] = value;
                }
            }

            return destination;
        }

        /// <inheritdoc/>
        public TEntity MapToSource(ExpandoObject destination)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            var allDestinationValues = (IDictionary<string, object>)destination;

            var result = new TEntity
            {
                Id = allDestinationValues[nameof(IIdentifiable.Id)].ToString(),
            };

            if (result is IETaggable taggableEntity)
            {
                taggableEntity.ETag = allDestinationValues[nameof(IETaggable.ETag)].ToString();
            }

            if (result is ITimestamped timestampedEntity && allDestinationValues.ContainsKey(nameof(ITimestamped.Timestamp)))
            {
                timestampedEntity.Timestamp = new DateTimeOffset((DateTime)allDestinationValues[nameof(ITimestamped.Timestamp)]);
            }

            foreach (var property in EntityAttributes.GetAttributeProperties<TEntity>())
            {
                if (allDestinationValues.ContainsKey(property.Name))
                {
                    var dbValue = allDestinationValues[property.Name];
                    if (dbValue != null)
                    {
                        if (!PetaPocoEntityMapper.IsNativelySupportedAsProperty(property.PropertyType))
                        {
                            property.SetValue(result, JsonSerializer.Deserialize(allDestinationValues[property.Name].ToString(), property.PropertyType));
                        }
                        else
                        {
                            property.SetValueWithCastOperators(result, dbValue);
                        }
                    }
                }
            }

            return result;
        }
    }
}
