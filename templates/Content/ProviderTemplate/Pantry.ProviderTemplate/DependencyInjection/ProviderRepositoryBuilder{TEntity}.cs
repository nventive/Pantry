using System;
using Microsoft.Extensions.DependencyInjection;
using Pantry.DependencyInjection;

namespace Pantry.ProviderTemplate.DependencyInjection
{
    /// <summary>
    /// <see cref="IProviderRepositoryBuilder{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class ProviderRepositoryBuilder<TEntity> : RepositoryBuilder, IProviderRepositoryBuilder<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderRepositoryBuilder{TEntity}"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        public ProviderRepositoryBuilder(IServiceCollection services)
            : base(services, typeof(TEntity))
        {
        }
    }
}
