using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace Pantry.Redis
{
    /// <summary>
    /// Static values for <see cref="RedisEntityMapper{T}"/>.
    /// </summary>
    public static class RedisEntityMapper
    {
        private static readonly HashSet<Type> SupportedPropertyTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(byte[]),
            typeof(bool),
            typeof(bool?),
            typeof(double),
            typeof(double?),
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?),
            typeof(uint),
            typeof(uint?),
            typeof(ulong),
            typeof(ulong?),
        };

        /// <summary>
        /// Returns true if <paramref name="type"/> is supported as a <see cref="RedisValue"/> property natively.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true is it is, false otherwise.</returns>
        public static bool IsNativelySupportedAsProperty(Type type) => SupportedPropertyTypes.Contains(type);
    }
}
