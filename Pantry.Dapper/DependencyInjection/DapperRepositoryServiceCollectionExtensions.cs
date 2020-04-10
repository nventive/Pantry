using Pantry;
using Pantry.Dapper;
using Pantry.Dapper.DependencyInjection;
using Pantry.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class DapperRepositoryServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default repository implemntation acked by Dapper implementation.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IDapperRepositoryBuilder{TEntity}"/>.</returns>
        public static IDapperRepositoryBuilder<TEntity> AddDapperRepository<TEntity>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            => services.AddDapperRepository<TEntity, DapperRepository<TEntity>>();

        /// <summary>
        /// Adds a custom repository backed by Dapper implementation.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TRepository">The repository type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IDapperRepositoryBuilder{TEntity}"/>.</returns>
        public static IDapperRepositoryBuilder<TEntity> AddDapperRepository<TEntity, TRepository>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            where TRepository : DapperRepository<TEntity>
        {
            services.TryAddIdGeneratorFor<TEntity>();
            services.TryAddETagGeneratorFor<TEntity>();
            services.TryAddTimestampProvider();

            services.TryAddAsSelfAndAllInterfaces<TRepository>();

            return new DapperRepositoryBuilder<TEntity>(services);
        }
    }
}
