using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry;
using Pantry.DependencyInjection;
using Pantry.Redis;
using Pantry.Redis.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class RedisRepositoryServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default repository implemntation backed by Redis implementation.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IRedisRepositoryBuilder{TEntity}"/>.</returns>
        public static IRedisRepositoryBuilder<TEntity> AddRedisRepository<TEntity>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            => services.AddRedisRepository<TEntity, RedisRepository<TEntity>>();

        /// <summary>
        /// Adds a custom repository backed by Redis implementation.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TRepository">The repository type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IRedisRepositoryBuilder{TEntity}"/>.</returns>
        public static IRedisRepositoryBuilder<TEntity> AddRedisRepository<TEntity, TRepository>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            where TRepository : RedisRepository<TEntity>
        {
            services.TryAddIdGeneratorFor<TEntity>();
            services.TryAddETagGeneratorFor<TEntity>();
            services.TryAddTimestampProvider();

            services.TryAddSingleton<IRedisEntityMapper<TEntity>, RedisEntityMapper<TEntity>>();

            var allInterfaces = services.TryAddAsSelfAndAllInterfaces<TRepository>();

            return new RedisRepositoryBuilder<TEntity>(services, allInterfaces);
        }
    }
}
