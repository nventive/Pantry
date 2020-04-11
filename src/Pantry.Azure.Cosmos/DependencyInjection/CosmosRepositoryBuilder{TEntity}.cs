using System;
using Microsoft.Extensions.DependencyInjection;
using Pantry.DependencyInjection;

namespace Pantry.Azure.Cosmos.DependencyInjection
{
    /// <summary>
    /// <see cref="ICosmosRepositoryBuilder{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class CosmosRepositoryBuilder<TEntity> : RepositoryBuilder, ICosmosRepositoryBuilder<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosRepositoryBuilder{TEntity}"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        public CosmosRepositoryBuilder(IServiceCollection services)
            : base(services, typeof(TEntity))
        {
        }
    }
}
