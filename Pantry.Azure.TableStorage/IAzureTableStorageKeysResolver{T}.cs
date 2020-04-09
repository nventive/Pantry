namespace Pantry.Azure.TableStorage
{
    /// <summary>
    /// Resolve Table Storage Key based on the entity Id.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public interface IAzureTableStorageKeysResolver<T>
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
    }
}
