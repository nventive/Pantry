using Pantry.DependencyInjection;

namespace Pantry.Azure.TableStorage.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder{TEntity}"/> for Azure Table Storage.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IAzureTableStorageRepositoryBuilder<TEntity> : IRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
