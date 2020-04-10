using System.Collections.Concurrent;
using System.Collections.Generic;
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
        /// <param name="tokenEncoder">The continuation token encoder.</param>
        public ConcurrentDictionaryAllQueryHandler(
            ConcurrentDictionary<string, TEntity> storage,
            IContinuationTokenEncoder<LimitOffsetContinuationToken> tokenEncoder)
            : base(storage, tokenEncoder)
        {
        }

        /// <inheritdoc/>
        protected override IEnumerable<TEntity> GetFilteredEnumeration(AllQuery<TEntity> query) => Storage.Values;
    }
}
