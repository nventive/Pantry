using Pantry.Queries;

namespace Pantry.InMemory.Queries
{
    /// <summary>
    /// Handler for queries destined for <see cref="ConcurrentDictionaryRepository{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The repository entity type.</typeparam>
    /// <typeparam name="TResult">The query result type.</typeparam>
    /// <typeparam name="TQuery">The query type.</typeparam>
    public interface IConcurrentDictionaryQueryHandler<TEntity, TResult, TQuery> : IQueryHandler<TEntity, TResult, TQuery>, IConcurrentDictionaryQueryHandler
        where TEntity : class, IIdentifiable
        where TQuery : IQuery<TResult>
    {
    }
}
