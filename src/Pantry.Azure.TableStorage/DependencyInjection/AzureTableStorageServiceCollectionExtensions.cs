﻿using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry;
using Pantry.Azure.TableStorage;
using Pantry.Azure.TableStorage.DependencyInjection;
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
            services.TryAddIdGeneratorFor<TEntity>();
            services.TryAddContinuationTokenEncoderFor<TableContinuationToken>();
            services.TryAddSingleton<IDynamicTableEntityMapper<TEntity>, DynamicTableEntityMapper<TEntity>>();

            var allInterfaces = services.TryAddAsSelfAndAllInterfaces<TRepository>();

            return new AzureTableStorageRepositoryBuilder<TEntity>(services, allInterfaces);
        }
    }
}
