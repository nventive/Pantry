using System;
using Microsoft.Azure.Cosmos;

namespace Pantry.Azure.Cosmos
{
    /// <summary>
    /// Encapsulates <see cref="Microsoft.Azure.Cosmos.Container"/> for easier Dependency Injection.
    /// </summary>
    /// <typeparam name="TEntity">The type related to the CosmosContainer itself. Probably an entity.</typeparam>
    public class CosmosContainerFor<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosContainerFor{T}"/> class.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> To encapsulate.</param>
        public CosmosContainerFor(Container container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Cosmos.Container"/>.
        /// </summary>
        public Container Container { get; }
    }
}
