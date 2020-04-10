namespace Pantry.Queries
{
    /// <summary>
    /// Query for entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IQuery<TEntity> : IQuery<TEntity, TEntity>
    {
    }
}
