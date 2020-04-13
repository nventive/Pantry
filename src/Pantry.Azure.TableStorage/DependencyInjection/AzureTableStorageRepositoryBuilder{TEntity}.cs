using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Pantry.DependencyInjection;

namespace Pantry.Azure.TableStorage.DependencyInjection
{
    /// <summary>
    /// <see cref="IAzureTableStorageRepositoryBuilder{T}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class AzureTableStorageRepositoryBuilder<TEntity> : RepositoryBuilder<TEntity>, IAzureTableStorageRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageRepositoryBuilder{T}"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="registeredRepositoryInterfaces">The list of registered repository interfaces.</param>
        public AzureTableStorageRepositoryBuilder(
            IServiceCollection services,
            IEnumerable<Type> registeredRepositoryInterfaces)
            : base(services, registeredRepositoryInterfaces)
        {
        }
    }
}
