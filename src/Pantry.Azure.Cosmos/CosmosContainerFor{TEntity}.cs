using System;
using Azure.Cosmos;

namespace Pantry.Azure.Cosmos
{
    /// <summary>
    /// Encapsulates <see cref="global::Azure.Cosmos.CosmosContainer"/> for easier Dependency Injection.
    /// </summary>
    /// <typeparam name="TEntity">The type related to the CosmosContainer itself. Probably an entity.</typeparam>
    public class CosmosContainerFor<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosContainerFor{T}"/> class.
        /// </summary>
        /// <param name="cosmosContainer">The <see cref="CosmosContainer"/> To encapsulate.</param>
        public CosmosContainerFor(CosmosContainer cosmosContainer)
        {
            CosmosContainer = cosmosContainer ?? throw new ArgumentNullException(nameof(cosmosContainer));
        }

        /// <summary>
        /// Gets the <see cref="global::Azure.Cosmos.CosmosContainer"/>.
        /// </summary>
        public CosmosContainer CosmosContainer { get; }
    }
}
