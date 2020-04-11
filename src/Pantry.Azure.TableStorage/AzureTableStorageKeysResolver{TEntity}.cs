using System;

namespace Pantry.Azure.TableStorage
{
    /// <summary>
    /// <see cref="IAzureTableStorageKeysResolver{T}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class AzureTableStorageKeysResolver<TEntity> : IAzureTableStorageKeysResolver<TEntity>
    {
        /// <inheritdoc/>
        public (string partitionKey, string rowKey) GetStorageKeys(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return (id, id);
        }

        /// <inheritdoc/>
        public string GetEntityId(string partitionKey, string rowKey) => rowKey;
    }
}
