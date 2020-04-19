using System;
using PetaPoco;

namespace Pantry.PetaPoco
{
    /// <summary>
    /// Encapsulates <see cref="IDatabase"/> for easier Dependency Injection.
    /// </summary>
    /// <typeparam name="TEntity">The type related to the IDatabase itself. Probably an entity.</typeparam>
    public class PetaPocoDatabaseFor<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PetaPocoDatabaseFor{T}"/> class.
        /// </summary>
        /// <param name="database">The <see cref="IDatabase"/> to encapsulate.</param>
        public PetaPocoDatabaseFor(IDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// Gets the <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase Database { get; }
    }
}
