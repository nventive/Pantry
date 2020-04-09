using System;
using Microsoft.Azure.Cosmos.Table;

namespace Pantry.Azure.TableStorage
{
    /// <summary>
    /// Encapsulates <see cref="CloudTable"/> for easier Dependency Injection.
    /// </summary>
    /// <typeparam name="TEntity">The type related to the CloudTable itself. Probably an entity.</typeparam>
    public class CloudTableFor<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloudTableFor{T}"/> class.
        /// </summary>
        /// <param name="cloudTable">The <see cref="CloudTable"/> To encapsulate.</param>
        public CloudTableFor(CloudTable cloudTable)
        {
            CloudTable = cloudTable ?? throw new ArgumentNullException(nameof(cloudTable));
        }

        /// <summary>
        /// Gets the <see cref="CloudTable"/>.
        /// </summary>
        public CloudTable CloudTable { get; }
    }
}
