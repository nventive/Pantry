using Pantry.DependencyInjection;

namespace Pantry.Redis.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder{TEntity}"/> for Redis.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRedisRepositoryBuilder<TEntity> : IRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
