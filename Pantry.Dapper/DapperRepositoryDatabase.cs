using Dapper;

namespace Pantry.Dapper
{
    /// <summary>
    /// Dapper Rainbow <see cref="Database{TDatabase}"/> implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class DapperRepositoryDatabase<TEntity> : Database<DapperRepositoryDatabase<TEntity>>
    {
        /// <summary>
        /// Gets or sets the entity table.
        /// </summary>
        public Table<TEntity, string>? Table { get; set; }
    }
}
