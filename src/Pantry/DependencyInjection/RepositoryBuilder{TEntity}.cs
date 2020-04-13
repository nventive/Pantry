using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Pantry.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class RepositoryBuilder<TEntity> : IRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryBuilder{TEntity}"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="registeredRepositoryInterfaces">The list of registered repository interfaces.</param>
        public RepositoryBuilder(
            IServiceCollection services,
            IEnumerable<Type> registeredRepositoryInterfaces)
        {
            Services = services;
            RegisteredRepositoryInterfaces = (registeredRepositoryInterfaces ?? Enumerable.Empty<Type>()).ToList();
        }

        /// <inheritdoc/>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the list of interface type registered as a repo.
        /// </summary>
        public IList<Type> RegisteredRepositoryInterfaces { get; }
    }
}
