using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Pantry.DependencyInjection;

namespace Pantry.Redis.DependencyInjection
{
    /// <summary>
    /// <see cref="IRedisRepositoryBuilder{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class RedisRepositoryBuilder<TEntity> : RepositoryBuilder<TEntity>, IRedisRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisRepositoryBuilder{TEntity}"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="registeredRepositoryInterfaces">The list of registered repository interfaces.</param>
        public RedisRepositoryBuilder(IServiceCollection services, IEnumerable<Type> registeredRepositoryInterfaces)
            : base(services, registeredRepositoryInterfaces)
        {
        }
    }
}
