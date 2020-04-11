using System;
using Microsoft.Extensions.DependencyInjection;

namespace Pantry.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder"/> default implementation.
    /// </summary>
    public class RepositoryBuilder : IRepositoryBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryBuilder"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="entityType">The entity type.</param>
        public RepositoryBuilder(
            IServiceCollection services,
            Type entityType)
        {
            Services = services;
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        }

        /// <inheritdoc/>
        public IServiceCollection Services { get; }

        /// <inheritdoc/>
        public Type EntityType { get; }
    }
}
