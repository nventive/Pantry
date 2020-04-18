using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry;
using Pantry.Redis;
using Pantry.Redis.DependencyInjection;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IRedisRepositoryBuilder{TEntity}"/> extension methods.
    /// </summary>
    public static class RedisRepositoryBuilderExtensions
    {
        /// <summary>
        /// Configure the Redis Repository to use the <see cref="IDatabaseAsync"/>
        /// resolved by the factory.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IRedisRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="databaseFactory">The <see cref="IDatabaseAsync"/> factory.</param>
        /// <returns>The udpated <see cref="IRedisRepositoryBuilder{TEntity}"/>.</returns>
        public static IRedisRepositoryBuilder<TEntity> WithRedisDatabaseFactory<TEntity>(
            this IRedisRepositoryBuilder<TEntity> builder,
            Func<IServiceProvider, IDatabaseAsync> databaseFactory)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddTransient(sp => new RedisDatabaseFor<TEntity>(databaseFactory(sp)));
            return builder;
        }

        /// <summary>
        /// Configure the Redis Repository to use a <paramref name="connectionString"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IRedisRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The udpated <see cref="IRedisRepositoryBuilder{TEntity}"/>.</returns>
        public static IRedisRepositoryBuilder<TEntity> WithConnectionString<TEntity>(
            this IRedisRepositoryBuilder<TEntity> builder,
            string connectionString)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(connectionString));
            builder.Services.TryAddTransient(
                sp => new RedisDatabaseFor<TEntity>(sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()));
            return builder;
        }

        /// <summary>
        /// Configure the Redis Repository to use the <paramref name="connectionStringName"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IRedisRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="connectionStringName">The name of the connection string.</param>
        /// <returns>The udpated <see cref="IRedisRepositoryBuilder{TEntity}"/>.</returns>
        public static IRedisRepositoryBuilder<TEntity> WithConnectionStringNamed<TEntity>(
            this IRedisRepositoryBuilder<TEntity> builder,
            string connectionStringName)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddSingleton<IConnectionMultiplexer>(
                sp => ConnectionMultiplexer.Connect(
                    sp.GetRequiredService<IConfiguration>().GetConnectionString(connectionStringName)));
            builder.Services.TryAddTransient(
                sp => new RedisDatabaseFor<TEntity>(sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()));
            return builder;
        }
    }
}
