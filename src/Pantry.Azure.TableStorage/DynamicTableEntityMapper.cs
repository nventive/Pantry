using System;
using System.Collections.Generic;

namespace Pantry.Azure.TableStorage
{
    /// <summary>
    /// Static values for <see cref="DynamicTableEntityMapper{T}"/>.
    /// </summary>
    public static class DynamicTableEntityMapper
    {
        private static readonly HashSet<Type> SupportedPropertyTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(byte[]),
            typeof(bool),
            typeof(bool?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
            typeof(double),
            typeof(double?),
            typeof(Guid),
            typeof(Guid?),
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?),
        };

        /// <summary>
        /// Returns true if <paramref name="type"/> is supported as a table property natively.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true is it is, false otherwise.</returns>
        public static bool IsNativelySupportedAsProperty(Type type) => SupportedPropertyTypes.Contains(type);
    }
}
