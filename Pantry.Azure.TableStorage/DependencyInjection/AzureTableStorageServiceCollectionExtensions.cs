using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry;
using Pantry.Azure.TableStorage;
using Pantry.Azure.TableStorage.DependencyInjection;
using Pantry.Azure.TableStorage.Queries;
using Pantry.Continuation;
using Pantry.DependencyInjection;
using Pantry.Mapping;
using Pantry.Queries;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class AzureTableStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default repository implementation backed by Azure Table Storage.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IAzureTableStorageRepositoryBuilder{T}"/>.</returns>
        public static IAzureTableStorageRepositoryBuilder<TEntity> AddAzureTableStorageRepository<TEntity>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            => services.AddAzureTableStorageRepository<TEntity, AzureTableStorageRepository<TEntity>>();

        /// <summary>
        /// Adds a custom repository backed by Azure Table Storage.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TRepository">The repository type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IAzureTableStorageRepositoryBuilder{T}"/>.</returns>
        public static IAzureTableStorageRepositoryBuilder<TEntity> AddAzureTableStorageRepository<TEntity, TRepository>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
        {
            services.TryAddSingleton<IIdGenerator<TEntity>, IdGenerator<TEntity>>();
            services.TryAddSingleton<IAzureTableStorageKeysResolver<TEntity>, AzureTableStorageKeysResolver<TEntity>>();
            services.TryAddSingleton<IMapper<TEntity, DynamicTableEntity>, DynamicTableEntityMapper<TEntity>>();
            services.TryAddSingleton<IContinuationTokenEncoder<TableContinuationToken>, Base64JsonContinuationTokenEncoder<TableContinuationToken>>();

            services.TryAddSingleton<IQueryHandlerExecutor<TEntity, IAzureTableStorageQueryHandler>, ServiceProviderQueryHandlerExecutor<TEntity, IAzureTableStorageQueryHandler>>();
            services.AddQueryHandler<AzureTableStorageAllQueryHandler<TEntity>, IAzureTableStorageQueryHandler>();
            services.AddQueryHandler<AzureTableStorageMirrorQueryHandler<TEntity>, IAzureTableStorageQueryHandler>();
            services.TryAddAsSelfAndAllInterfaces<TRepository>();

            return new AzureTableStorageRepositoryBuilder<TEntity>(services);
        }
    }
}
