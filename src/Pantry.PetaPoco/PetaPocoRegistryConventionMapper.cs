using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using PetaPoco;

namespace Pantry.PetaPoco
{
    /// <summary>
    /// PetaPoco <see cref="ConventionMapper"/> extension for repository support.
    /// Will convert nested objects and collections to JSON.
    /// </summary>
    public class PetaPocoRegistryConventionMapper : ConventionMapper
    {
        private readonly JsonSerializerOptions? _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PetaPocoRegistryConventionMapper"/> class.
        /// </summary>
        public PetaPocoRegistryConventionMapper()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PetaPocoRegistryConventionMapper"/> class.
        /// </summary>
        /// <param name="jsonSerializerOptions">The <see cref="JsonSerializerOptions"/> to use.</param>
        public PetaPocoRegistryConventionMapper(JsonSerializerOptions? jsonSerializerOptions)
        {
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        /// <inheritdoc/>
        [SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "PetaPoco API design.")]
        public override Func<object, object?>? GetFromDbConverter(PropertyInfo targetProperty, Type sourceType)
        {
            if (targetProperty is null)
            {
                throw new ArgumentNullException(nameof(targetProperty));
            }

            if (sourceType is null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            var converter = base.GetFromDbConverter(targetProperty, sourceType);
            if (converter is null)
            {
                if (ShouldSerializeAsJson(targetProperty.PropertyType))
                {
                    return (value) => value == null ? null : JsonSerializer.Deserialize(value.ToString(), targetProperty.PropertyType, _jsonSerializerOptions);
                }

                if (targetProperty.PropertyType == typeof(DateTimeOffset) && sourceType == typeof(string))
                {
                    return (value) => string.IsNullOrEmpty((string?)value) ? default : DateTimeOffset.Parse((string)value, CultureInfo.InvariantCulture);
                }

                if (targetProperty.PropertyType == typeof(DateTimeOffset?) && sourceType == typeof(string))
                {
                    return (value) => string.IsNullOrEmpty((string?)value) ? (DateTimeOffset?)null : DateTimeOffset.Parse((string)value, CultureInfo.InvariantCulture);
                }
            }

            return converter;
        }

        /// <inheritdoc/>
        [SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "PetaPoco API design.")]
        public override Func<object, object>? GetToDbConverter(PropertyInfo sourceProperty)
        {
            if (sourceProperty is null)
            {
                throw new ArgumentNullException(nameof(sourceProperty));
            }

            var converter = base.GetToDbConverter(sourceProperty);
            if (converter is null)
            {
                if (ShouldSerializeAsJson(sourceProperty.PropertyType))
                {
                    return (value) => JsonSerializer.Serialize(value, _jsonSerializerOptions);
                }
            }

            return converter;
        }

        private bool ShouldSerializeAsJson(Type propertyType) => !propertyType.IsValueType && propertyType != typeof(string);
    }
}
