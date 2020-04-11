using System;
using Microsoft.Extensions.DependencyInjection;

namespace Pantry.DependencyInjection
{
    /// <summary>
    /// Dependency Registration builder.
    /// </summary>
    public interface IRepositoryBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/>.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public Type EntityType { get; }
    }
}
