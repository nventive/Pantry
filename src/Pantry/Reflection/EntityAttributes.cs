using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pantry.Reflection
{
    /// <summary>
    /// Helper class for entity attributes.
    /// </summary>
    public static class EntityAttributes
    {
        /// <summary>
        /// Returns the list of attributes <see cref="PropertyInfo"/> in <typeparamref name="TEntity"/>.
        /// Attributes are defined as:
        /// - Properties that are Read/Write
        /// - Not in standard entity interaces (<see cref="IIdentifiable"/>, <see cref="IETaggable"/> and <see cref="ITimestamped"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <returns>The list of properties.</returns>
        public static IEnumerable<PropertyInfo> GetAttributeProperties<TEntity>()
            => GetAttributeProperties(typeof(TEntity));

        /// <summary>
        /// Returns the list of attributes <see cref="PropertyInfo"/> in <paramref name="entityType"/>.
        /// Attributes are defined as:
        /// - Properties that are Read/Write
        /// - Not in standard entity interaces (<see cref="IIdentifiable"/>, <see cref="IETaggable"/> and <see cref="ITimestamped"/>).
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The list of properties.</returns>
        public static IEnumerable<PropertyInfo> GetAttributeProperties(Type entityType)
        {
            if (entityType is null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            return entityType.GetProperties()
                .Where(x => x.CanRead && x.CanWrite)
                .Where(x => x.Name != nameof(IIdentifiable.Id) && x.Name != nameof(IETaggable.ETag) && x.Name != nameof(ITimestamped.Timestamp))
                .ToList();
        }
    }
}
