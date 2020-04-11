using Pantry.DependencyInjection;

namespace Pantry.ProviderTemplate.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder"/> for Provider.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IProviderRepositoryBuilder<TEntity> : IRepositoryBuilder
    {
    }
}
