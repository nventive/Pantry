using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using Pantry.Mapping;

namespace Pantry.Azure.TableStorage
{
    /// <summary>
    /// Mapper for <see cref="DynamicTableEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entityt type.</typeparam>
    public interface IDynamicTableEntityMapper<TEntity> : IMapper<TEntity, DynamicTableEntity>
        where TEntity : class, IIdentifiable, new()
    {
        /// <summary>
        /// Gets the partitionKey and rowKey from the <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>The partition key and the row key.</returns>.
        (string partitionKey, string rowKey) GetStorageKeys(string id);

        /// <summary>
        /// Gets the entity id from <paramref name="partitionKey"/> and <paramref name="rowKey"/>.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <returns>The entity id.</returns>.
        string GetEntityId(string partitionKey, string rowKey);

        /// <summary>
        /// Resolve the original <paramref name="propertyPath"/> to the
        /// querying property paths.
        /// </summary>
        /// <param name="propertyPath">The original property path.</param>
        /// <returns>The querying property paths.</returns>
        IEnumerable<string> ResolveQueryPropertyPaths(string propertyPath);
    }
}
