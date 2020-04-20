using Pantry;
using Pantry.Continuation;
using Pantry.DependencyInjection;
using Pantry.PetaPoco;
using Pantry.PetaPoco.DependencyInjection;
using PetaPoco;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class PetaPocoRepositoryServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default repository implemntation backed by PetaPoco implementation.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IPetaPocoRepositoryBuilder{TEntity}"/>.</returns>
        public static IPetaPocoRepositoryBuilder<TEntity> AddPetaPocoRepository<TEntity>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            => services.AddPetaPocoRepository<TEntity, PetaPocoRepository<TEntity>>();

        /// <summary>
        /// Adds a custom repository backed by PetaPoco implementation.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TRepository">The repository type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IPetaPocoRepositoryBuilder{TEntity}"/>.</returns>
        public static IPetaPocoRepositoryBuilder<TEntity> AddPetaPocoRepository<TEntity, TRepository>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            where TRepository : PetaPocoRepository<TEntity>
        {
            services.TryAddIdGeneratorFor<TEntity>();
            services.TryAddETagGeneratorFor<TEntity>();
            services.TryAddTimestampProvider();
            services.TryAddContinuationTokenEncoderFor<PageContinuationToken>();

            Mappers.Register(typeof(TEntity).Assembly, new PetaPocoRegistryConventionMapper());

            var allInterfaces = services.TryAddAsSelfAndAllInterfaces<TRepository>();

            return new PetaPocoRepositoryBuilder<TEntity>(services, allInterfaces);
        }
    }
}
