using System;
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
        public HashEntry[] MapToDestination(TEntity source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public TEntity MapToSource(HashEntry[] destination)
        {
            throw new NotImplementedException();
        }
    }
}
