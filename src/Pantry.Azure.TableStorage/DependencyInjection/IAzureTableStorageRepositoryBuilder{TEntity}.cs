using Pantry.DependencyInjection;

namespace Pantry.Azure.TableStorage.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder"/> for Azure Table Storage.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IAzureTableStorageRepositoryBuilder<TEntity> : IRepositoryBuilder
        where TEntity : class, IIdentifiable
    {
    }
}
