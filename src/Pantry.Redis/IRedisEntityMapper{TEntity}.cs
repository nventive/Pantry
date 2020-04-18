using System.Collections.Generic;
using Pantry.Mapping;
using StackExchange.Redis;

namespace Pantry.Redis
{
    /// <summary>
    /// Mapper for Redis entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRedisEntityMapper<TEntity> : IMapper<TEntity, IEnumerable<HashEntry>>
        where TEntity : class, IIdentifiable, new()
    {
        /// <summary>
        /// Gets the key for the corresponding <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>The <see cref="RedisKey"/>.</returns>
        RedisKey GetRedisKey(string id);

        /// <summary>
        /// Returns the name of the ETag field.
        /// </summary>
        /// <returns>The <see cref="RedisValue"/> ETag field.</returns>
        RedisValue GetETagField();
    }
}
