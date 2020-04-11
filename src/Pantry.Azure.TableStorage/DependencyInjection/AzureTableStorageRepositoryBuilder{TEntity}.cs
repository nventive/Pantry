using Microsoft.Extensions.DependencyInjection;
using Pantry.DependencyInjection;

namespace Pantry.Azure.TableStorage.DependencyInjection
{
    /// <summary>
    /// <see cref="IAzureTableStorageRepositoryBuilder{T}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class AzureTableStorageRepositoryBuilder<TEntity> : RepositoryBuilder, IAzureTableStorageRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageRepositoryBuilder{T}"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        public AzureTableStorageRepositoryBuilder(IServiceCollection services)
            : base(services, typeof(TEntity))
        {
        }
    }
}
