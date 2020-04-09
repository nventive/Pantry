using Pantry.Queries;

namespace Pantry.Azure.TableStorage.Queries
{
    /// <summary>
    /// Handler for queries destined for <see cref="AzureTableStorageRepository{T}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The repository entity type.</typeparam>
    /// <typeparam name="TResult">The query result type.</typeparam>
    /// <typeparam name="TQuery">The query type.</typeparam>
    public interface IAzureTableStorageQueryHandler<TEntity, TResult, TQuery> : IQueryHandler<TEntity, TResult, TQuery>, IAzureTableStorageQueryHandler
        where TEntity : class, IIdentifiable
        where TQuery : IQuery<TResult>
    {
    }
}
