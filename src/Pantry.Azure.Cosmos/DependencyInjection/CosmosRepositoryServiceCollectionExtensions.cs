using Pantry;
using Pantry.Azure.Cosmos;
using Pantry.Azure.Cosmos.DependencyInjection;
using Pantry.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class CosmosRepositoryServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default repository implemntation backed by Cosmos implementation.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</returns>
        public static ICosmosRepositoryBuilder<TEntity> AddCosmosRepository<TEntity>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            => services.AddCosmosRepository<TEntity, CosmosRepository<TEntity>>();

        /// <summary>
        /// Adds a custom repository backed by Cosmos implementation.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TRepository">The repository type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="ICosmosRepositoryBuilder{TEntity}"/>.</returns>
        public static ICosmosRepositoryBuilder<TEntity> AddCosmosRepository<TEntity, TRepository>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            where TRepository : CosmosRepository<TEntity>
        {
            services.TryAddIdGeneratorFor<TEntity>();

            var allInterfaces = services.TryAddAsSelfAndAllInterfaces<TRepository>();

            return new CosmosRepositoryBuilder<TEntity>(services, allInterfaces);
        }
    }
}
