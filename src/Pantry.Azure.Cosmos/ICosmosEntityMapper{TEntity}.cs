using Microsoft.Azure.Cosmos;
using Pantry.Mapping;

namespace Pantry.Azure.Cosmos
{
    /// <summary>
    /// Mapper for cosmos entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface ICosmosEntityMapper<TEntity> : IMapper<TEntity, CosmosDocument>
        where TEntity : class, IIdentifiable, new()
    {
        /// <summary>
        /// Gets the partition key for the corresponding <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>The <see cref="PartitionKey"/>.</returns>
        PartitionKey GetPartitionKey(string id);

        /// <summary>
        /// Gets the <see cref="CosmosDocument.EntityType"/> value.
        /// </summary>
        /// <returns>The entity type.</returns>
        string GetEntityType();

        /// <summary>
        /// Resolve the original <paramref name="propertyPath"/> to the
        /// querying property paths.
        /// </summary>
        /// <param name="propertyPath">The original property path.</param>
        /// <returns>The querying property paths.</returns>
        string ResolveQueryPropertyPaths(string propertyPath);
    }
}
