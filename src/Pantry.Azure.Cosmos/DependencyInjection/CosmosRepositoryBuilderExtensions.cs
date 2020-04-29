using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Pantry;
using Pantry.Azure.Cosmos;
using Pantry.Azure.Cosmos.Configuration;
using Pantry.Azure.Cosmos.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="ICosmosRepositoryBuilder{TEntity}"/> extension methods.
    /// </summary>
    public static class CosmosRepositoryBuilderExtensions
    {
        /// <summary>
        /// Configure the Cosmos Repository to use the <see cref="Container"/>
        /// resolved by the factory.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="cosmosContainerFactory">The <see cref="Container"/> factory.</param>
        /// <returns>The udpated <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</returns>
        public static ICosmosRepositoryBuilder<TEntity> WithCosmosContainerFactory<TEntity>(
            this ICosmosRepositoryBuilder<TEntity> builder,
            Func<IServiceProvider, Container> cosmosContainerFactory)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddTransient(sp => new CosmosContainerFor<TEntity>(cosmosContainerFactory(sp)));
            return builder;
        }

        /// <summary>
        /// Configure the Cosmos Repository to use the <paramref name="connectionString"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="configure">Options configuration.</param>
        /// <returns>The udpated <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</returns>
        public static ICosmosRepositoryBuilder<TEntity> WithConnectionString<TEntity>(
            this ICosmosRepositoryBuilder<TEntity> builder,
            string connectionString,
            Action<CosmosRepositoryOptions>? configure = null)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.Configure<CosmosRepositoryOptions>(opt => { configure?.Invoke(opt); });
            builder.Services.TryAddSingleton(sp => new CosmosContainerFactory(connectionString, sp.GetRequiredService<IOptions<CosmosRepositoryOptions>>()));
            builder.Services.TryAddTransient(sp => new CosmosContainerFor<TEntity>(sp.GetRequiredService<CosmosContainerFactory>().Container));

            return builder;
        }

        /// <summary>
        /// Configure the Cosmos Repository to use the <paramref name="connectionStringName"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="configure">Options configuration.</param>
        /// <returns>The udpated <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</returns>
        public static ICosmosRepositoryBuilder<TEntity> WithConnectionStringNamed<TEntity>(
            this ICosmosRepositoryBuilder<TEntity> builder,
            string connectionStringName,
            Action<CosmosRepositoryOptions>? configure = null)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.Configure<CosmosRepositoryOptions>(opt => { configure?.Invoke(opt); });
            builder.Services.TryAddSingleton(
                sp => new CosmosContainerFactory(
                    sp.GetRequiredService<IConfiguration>().GetConnectionString(connectionStringName),
                    sp.GetRequiredService<IOptions<CosmosRepositoryOptions>>()));
            builder.Services.TryAddTransient(sp => new CosmosContainerFor<TEntity>(sp.GetRequiredService<CosmosContainerFactory>().Container));

            return builder;
        }

        /// <summary>
        /// Sets a custom mapper for cosmos entities (as a Singleton).
        /// </summary>
        /// <typeparam name="TEntity">The repository entity type.</typeparam>
        /// <typeparam name="TMapper">The custom mapper type.</typeparam>
        /// <param name="builder">The <see cref="ICosmosRepositoryBuilder{T}"/>.</param>
        /// <returns>The updated <see cref="ICosmosRepositoryBuilder{T}"/>.</returns>
        public static ICosmosRepositoryBuilder<TEntity> WithCosmosEntityMapper<TEntity, TMapper>(
            this ICosmosRepositoryBuilder<TEntity> builder)
            where TEntity : class, IIdentifiable, new()
            where TMapper : class, ICosmosEntityMapper<TEntity>
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<ICosmosEntityMapper<TEntity>, TMapper>();
            return builder;
        }
    }
}
