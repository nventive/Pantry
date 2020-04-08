using Pantry.Traits;

namespace Pantry
{
    /// <summary>
    /// Create Read Update Delete Repository.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface ICrudRepository<T> : ICanAdd<T>, ICanGet<T>, ICanUpdate<T>, ICanDelete<T>
        where T : class, IIdentifiable
    {
    }
}
