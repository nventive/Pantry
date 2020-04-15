using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Pantry.DependencyInjection;

namespace Pantry.Azure.Cosmos.DependencyInjection
{
    /// <summary>
    /// <see cref="ICosmosRepositoryBuilder{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class CosmosRepositoryBuilder<TEntity> : RepositoryBuilder<TEntity>, ICosmosRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosRepositoryBuilder{TEntity}"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="registeredRepositoryInterfaces">The list of registered repository interfaces.</param>
        public CosmosRepositoryBuilder(IServiceCollection services, IEnumerable<Type> registeredRepositoryInterfaces)
            : base(services, registeredRepositoryInterfaces)
        {
        }
    }
}
