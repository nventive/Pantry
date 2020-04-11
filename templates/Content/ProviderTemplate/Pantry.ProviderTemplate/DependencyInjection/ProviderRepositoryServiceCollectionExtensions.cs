using Pantry;
using Pantry.ProviderTemplate;
using Pantry.ProviderTemplate.DependencyInjection;
using Pantry.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ProviderRepositoryServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default repository implemntation backed by Provider implementation.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IProviderRepositoryBuilder{TEntity}"/>.</returns>
        public static IProviderRepositoryBuilder<TEntity> AddProviderRepository<TEntity>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            => services.AddProviderRepository<TEntity, ProviderRepository<TEntity>>();

        /// <summary>
        /// Adds a custom repository backed by Provider implementation.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TRepository">The repository type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IProviderRepositoryBuilder{TEntity}"/>.</returns>
        public static IProviderRepositoryBuilder<TEntity> AddProviderRepository<TEntity, TRepository>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            where TRepository : ProviderRepository<TEntity>
        {
            services.TryAddIdGeneratorFor<TEntity>();

            services.TryAddAsSelfAndAllInterfaces<TRepository>();

            return new ProviderRepositoryBuilder<TEntity>(services);
        }
    }
}
