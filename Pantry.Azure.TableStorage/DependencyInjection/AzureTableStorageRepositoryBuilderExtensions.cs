using System;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry;
using Pantry.Azure.TableStorage;
using Pantry.Azure.TableStorage.DependencyInjection;
using Pantry.Mapping;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IAzureTableStorageRepositoryBuilder{T}"/> extension methods.
    /// </summary>
    public static class AzureTableStorageRepositoryBuilderExtensions
    {
        /// <summary>
        /// Use the connection string named <paramref name="connectionStringName"/>
        /// to instantiate the Azure Table Storage Repository.
        /// Configures the <see cref="CloudTableFor{T}"/> in the Dependency Injection.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IAzureTableStorageRepositoryBuilder{T}"/>.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="tableName">The table name, if any. Defaults to typeof(T).Name.</param>
        /// <param name="configureTableClient">Configures the <see cref="CloudTableClient"/>.</param>
        /// <returns>The updated <see cref="IAzureTableStorageRepositoryBuilder{T}"/>.</returns>
        public static IAzureTableStorageRepositoryBuilder<T> WithConnectionStringNamed<T>(
            this IAzureTableStorageRepositoryBuilder<T> builder,
            string connectionStringName,
            string? tableName = null,
            Action<IServiceProvider, CloudTableClient>? configureTableClient = null)
            where T : class, IIdentifiable
            => builder.WithConnectionString(
                connectionStringProvider: sp => sp.GetRequiredService<IConfiguration>().GetConnectionString(connectionStringName),
                tableName: tableName,
                configureTableClient: configureTableClient);

        /// <summary>
        /// Use the connection string to instantiate the Azure Table Storage Repository.
        /// Configures the <see cref="CloudTableFor{T}"/> in the Dependency Injection.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IAzureTableStorageRepositoryBuilder{T}"/>.</param>
        /// <param name="connectionStringProvider">Factory method to retrieve the connection string.</param>
        /// <param name="tableName">The table name, if any. Defaults to typeof(T).Name.</param>
        /// <param name="configureTableClient">Configures the <see cref="CloudTableClient"/>.</param>
        /// <returns>The updated <see cref="IAzureTableStorageRepositoryBuilder{T}"/>.</returns>
        public static IAzureTableStorageRepositoryBuilder<T> WithConnectionString<T>(
            this IAzureTableStorageRepositoryBuilder<T> builder,
            Func<IServiceProvider, string> connectionStringProvider,
            string? tableName = null,
            Action<IServiceProvider, CloudTableClient>? configureTableClient = null)
            where T : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddSingleton(sp =>
            {
                var connectionString = connectionStringProvider(sp);
                var cloudTableClient = CloudStorageAccount
                    .Parse(connectionString)
                    .CreateCloudTableClient();
                configureTableClient?.Invoke(sp, cloudTableClient);
                var table = cloudTableClient.GetTableReference(tableName ?? typeof(T).Name);
                table.CreateIfNotExists();

                return new CloudTableFor<T>(table);
            });

            return builder;
        }

        /// <summary>
        /// Sets a custom mapper for table entities (as a Singleton).
        /// </summary>
        /// <typeparam name="TEntity">The repository entity type.</typeparam>
        /// <typeparam name="TMapper">The custom mapper type.</typeparam>
        /// <param name="builder">The <see cref="IAzureTableStorageRepositoryBuilder{T}"/>.</param>
        /// <returns>The updated <see cref="IAzureTableStorageRepositoryBuilder{T}"/>.</returns>
        public static IAzureTableStorageRepositoryBuilder<TEntity> WithTableEntityMapper<TEntity, TMapper>(
            this IAzureTableStorageRepositoryBuilder<TEntity> builder)
            where TEntity : class, IIdentifiable
            where TMapper : class, IMapper<TEntity, DynamicTableEntity>
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<IMapper<TEntity, DynamicTableEntity>, TMapper>();
            return builder;
        }
    }
}
