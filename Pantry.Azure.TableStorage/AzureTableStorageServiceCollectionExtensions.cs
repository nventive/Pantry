using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry;
using Pantry.Azure.TableStorage;
using Pantry.Azure.TableStorage.Queries;
using Pantry.DependencyInjection;
using Pantry.Mapping;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class AzureTableStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a repository backed by Azure Table Storage.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        /// <param name="connectionStringName">The connection string name for the storage account.</param>
        /// <param name="tableName">The storage table name.</param>
        /// <returns>The <see cref="IRepositoryBuilder"/>.</returns>
        public static IRepositoryBuilder AddAzureTableStorageRepository<T>(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringName,
            string tableName)
            where T : class, IIdentifiable, new()
        {
            services.TryAddSingleton<IIdGenerator<T>, IdGenerator<T>>();

            services.TryAddSingleton(sp =>
            {
                var cloudTableClient = CloudStorageAccount.Parse(configuration.GetConnectionString(connectionStringName))
                    .CreateCloudTableClient();
                var table = cloudTableClient.GetTableReference(tableName);
                table.CreateIfNotExists();

                return new CloudTableFor<T>(table);
            });
            services.TryAddSingleton<ITableStorageKeysResolver<T>, DefaultTableStorageKeysResolver<T>>();
            services.TryAddSingleton<IMapper<T, DynamicTableEntity>, DynamicTableEntityMapper<T>>();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IAzureTableStorageQueryHandler, AzureTableStorageAllQueryHandler<T>>());

            services.TryRegisterAsSelfAndAllInterfaces<AzureTableStorageRepository<T>>();

            return new RepositoryBuilder(services, typeof(T));
        }
    }
}
