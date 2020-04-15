using Pantry.DependencyInjection;

namespace Pantry.Azure.Cosmos.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder{TEntity}"/> for Cosmos.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface ICosmosRepositoryBuilder<TEntity> : IRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
