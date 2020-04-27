namespace Pantry.Mediator.Repositories.Commands
{
    /// <summary>
    /// Standard command to update an entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public abstract class UpdateCommand<TEntity> : UpdateCommand<TEntity, TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
