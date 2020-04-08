﻿using System;

namespace Pantry.Azure.TableStorage
{
    /// <summary>
    /// <see cref="ITableStorageKeysResolver{T}"/> default implementation.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class DefaultTableStorageKeysResolver<T> : ITableStorageKeysResolver<T>
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
