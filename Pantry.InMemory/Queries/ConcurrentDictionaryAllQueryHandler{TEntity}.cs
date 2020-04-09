using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Pantry.Continuation;
using Pantry.Queries;

namespace Pantry.InMemory.Queries
{
    /// <summary>
    /// Handles <see cref="AllQuery{TResult}"/> for <see cref="ConcurrentDictionaryRepository{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The repository entity type.</typeparam>
    public class ConcurrentDictionaryAllQueryHandler<TEntity> : ConcurrentDictionaryQueryHandler<TEntity, AllQuery<TEntity>>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionaryAllQueryHandler{TEntity}"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        public ConcurrentDictionaryAllQueryHandler(
            ConcurrentDictionary<string, TEntity> storage)
            : base(storage)
        {
        }

        /// <inheritdoc/>
        public override async Task<IContinuationEnumerable<TEntity>> ExecuteAsync(AllQuery<TEntity> query, CancellationToken cancellationToken = default)
            => Storage.Values.ToContinuationEnumerable(query);
    }
}
