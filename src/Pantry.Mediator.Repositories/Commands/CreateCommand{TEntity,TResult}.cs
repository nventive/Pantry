namespace Pantry.Mediator.Repositories.Commands
{
    /// <summary>
    /// Standard command to create an entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public abstract class CreateCommand<TEntity, TResult> : IDomainCommand<TResult>, IRepositoryRequest
        where TEntity : class, IIdentifiable
    {
    }
}
