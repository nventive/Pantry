using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Continuation;
using Pantry.Queries;

namespace Pantry.InMemory.Queries
{
    /// <summary>
    /// Base class for <see cref="IConcurrentDictionaryQueryHandler{TEntity,TResult,TQuery}"/> implementations.
    /// </summary>
    /// <typeparam name="TEntity">The repository entity type.</typeparam>
    /// <typeparam name="TQuery">The query type.</typeparam>
    public abstract class ConcurrentDictionaryQueryHandler<TEntity, TQuery> : IConcurrentDictionaryQueryHandler<TEntity, TEntity, TQuery>
        where TEntity : class, IIdentifiable
        where TQuery : IQuery<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionaryQueryHandler{TEntity, TQuery}"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        public ConcurrentDictionaryQueryHandler(
            ConcurrentDictionary<string, TEntity> storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Gets the storage.
        /// </summary>
        protected ConcurrentDictionary<string, TEntity> Storage { get; }

        /// <inheritdoc/>
        public abstract Task<IContinuationEnumerable<TEntity>> ExecuteAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}
