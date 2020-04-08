using Pantry.Traits;

namespace Pantry
{
    /// <summary>
    /// CRUD Repository that supports queries.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IRepository<T> : ICrudRepository<T>, ICanQuery
        where T : class, IIdentifiable
    {
    }
}
