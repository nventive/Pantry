using System;
using StackExchange.Redis;

namespace Pantry.Redis
{
    /// <summary>
    /// Encapsulates <see cref="IDatabaseAsync"/> for easier Dependency Injection.
    /// </summary>
    /// <typeparam name="TEntity">The type related to the IDatabase itself. Probably an entity.</typeparam>
    public class RedisDatabaseFor<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisDatabaseFor{T}"/> class.
        /// </summary>
        /// <param name="database">The <see cref="IDatabaseAsync"/> To encapsulate.</param>
        public RedisDatabaseFor(IDatabaseAsync database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// Gets the <see cref="IDatabaseAsync"/>.
        /// </summary>
        public IDatabaseAsync Database { get; }
    }
}
