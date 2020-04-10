using System;
using System.Data;
using System.Data.Common;

namespace Pantry.Dapper
{
    /// <summary>
    /// Encapsulates <see cref="IDbConnection"/> for easier Dependency Injection.
    /// </summary>
    /// <typeparam name="TEntity">The type related to the IDbConnection itself. Probably an entity.</typeparam>
    public class DbConnectionFor<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionFor{TEntity}"/> class.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        public DbConnectionFor(DbConnection dbConnection)
        {
            DbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        /// <summary>
        /// Gets the <see cref="DbConnection"/>.
        /// </summary>
        public DbConnection DbConnection { get; }
    }
}
