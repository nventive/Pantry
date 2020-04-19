using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Pantry.DependencyInjection;

namespace Pantry.PetaPoco.DependencyInjection
{
    /// <summary>
    /// <see cref="IPetaPocoRepositoryBuilder{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class PetaPocoRepositoryBuilder<TEntity> : RepositoryBuilder<TEntity>, IPetaPocoRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PetaPocoRepositoryBuilder{TEntity}"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="registeredRepositoryInterfaces">The list of registered repository interfaces.</param>
        public PetaPocoRepositoryBuilder(IServiceCollection services, IEnumerable<Type> registeredRepositoryInterfaces)
            : base(services, registeredRepositoryInterfaces)
        {
        }
    }
}
