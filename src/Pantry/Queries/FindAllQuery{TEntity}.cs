namespace Pantry.Queries
{
    /// <summary>
    /// This can be used when implementing FindAll features in repository implementations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class FindAllQuery<TEntity> : Query<TEntity>
    {
    }
}
