using Pantry.DependencyInjection;

namespace Pantry.Azure.TableStorage.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder"/> for Azure Table Storage.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IAzureTableStorageRepositoryBuilder<T> : IRepositoryBuilder
        where T : class, IIdentifiable
    {
    }
}
