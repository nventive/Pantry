using System.Dynamic;
using Pantry.Mapping;

namespace Pantry.PetaPoco
{
    /// <summary>
    /// Mapper for PetaPoco entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IPetaPocoEntityMapper<TEntity> : IMapper<TEntity, ExpandoObject>
    {
        /// <summary>
        /// Gets the primary key for the corresponding <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>The primary key.</returns>
        object GetPrimaryKey(string id);

        /// <summary>
        /// Gets the primary key name.
        /// </summary>
        /// <returns>The primary key name.</returns>
        string GetPrimaryKeyName();

        /// <summary>
        /// Gets the table name.
        /// </summary>
        /// <returns>The table name.</returns>
        string GetTableName();
    }
}
