using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry;
using Pantry.Azure.Cosmos;
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
        /// Configure the Cosmos Repository to use a connection string.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="containerName">The container name.</param>
        /// <returns>The udpated <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</returns>
        public static ICosmosRepositoryBuilder<TEntity> WithConnectionString<TEntity>(
            this ICosmosRepositoryBuilder<TEntity> builder,
            string connectionString,
            string databaseName = "data",
            string containerName = "data")
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException(nameof(databaseName));
            }

            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            builder.WithCosmosContainerFactory(sp =>
            {
                // TODO: factory for CosmosClient / CosmosDatabase.
                var client = new CosmosClient(connectionString);
                client.CreateDatabaseIfNotExistsAsync(databaseName).ConfigureAwait(false).GetAwaiter().GetResult();
                var database = client.GetDatabase(databaseName);
                database.CreateContainerIfNotExistsAsync(containerName, "/id").ConfigureAwait(false).GetAwaiter().GetResult();
                return database.GetContainer(containerName);
            });
            return builder;
        }

        /// <summary>
        /// Configure the Cosmos Repository to use a connection string by name.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="connectionStringName">The name of the connection string.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="containerName">The container name.</param>
        /// <returns>The udpated <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</returns>
        public static ICosmosRepositoryBuilder<TEntity> WithConnectionStringNamed<TEntity>(
            this ICosmosRepositoryBuilder<TEntity> builder,
            string connectionStringName,
            string databaseName = "data",
            string containerName = "data")
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException(nameof(databaseName));
            }

            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            builder.WithCosmosContainerFactory(sp =>
            {
                var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(connectionStringName);
                // TODO: factory for CosmosClient / CosmosDatabase.
                var client = new CosmosClient(connectionString);
                client.CreateDatabaseIfNotExistsAsync(databaseName).ConfigureAwait(false).GetAwaiter().GetResult();
                var database = client.GetDatabase(databaseName);
                database.CreateContainerIfNotExistsAsync(containerName, "/id").ConfigureAwait(false).GetAwaiter().GetResult();
                return database.GetContainer(containerName);
            });
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
