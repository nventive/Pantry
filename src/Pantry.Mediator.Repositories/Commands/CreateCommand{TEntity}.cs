namespace Pantry.Mediator.Repositories.Commands
{
    /// <summary>
    /// Standard command to create entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public abstract class CreateCommand<TEntity> : CreateCommand<TEntity, TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
