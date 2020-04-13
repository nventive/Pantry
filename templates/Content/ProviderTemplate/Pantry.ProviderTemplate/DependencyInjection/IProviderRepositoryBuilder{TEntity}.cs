using Pantry.DependencyInjection;

namespace Pantry.ProviderTemplate.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder{TEntity}"/> for Provider.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IProviderRepositoryBuilder<TEntity> : IRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
