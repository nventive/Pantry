using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Pantry.DependencyInjection
{
    /// <summary>
    /// Dependency Registration builder.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/>.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the list of interface type registered as a repo.
        /// </summary>
        public IList<Type> RegisteredRepositoryInterfaces { get; }
    }
}
