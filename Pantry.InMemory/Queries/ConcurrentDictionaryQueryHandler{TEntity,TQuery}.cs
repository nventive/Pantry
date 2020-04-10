using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        /// <param name="tokenEncoder">The continuation token encoder.</param>
        public ConcurrentDictionaryQueryHandler(
            ConcurrentDictionary<string, TEntity> storage,
            IContinuationTokenEncoder<LimitOffsetContinuationToken> tokenEncoder)
        {
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            TokenEncoder = tokenEncoder ?? throw new ArgumentNullException(nameof(tokenEncoder));
        }

        /// <summary>
        /// Gets the storage.
        /// </summary>
        protected ConcurrentDictionary<string, TEntity> Storage { get; }

        /// <summary>
        /// Gets the <see cref="IContinuationTokenEncoder{LimitOffsetContinuationToken}"/>.
        /// </summary>
        protected IContinuationTokenEncoder<LimitOffsetContinuationToken> TokenEncoder { get; }

        /// <inheritdoc/>
        public virtual async Task<IContinuationEnumerable<TEntity>> ExecuteAsync(TQuery query, CancellationToken cancellationToken = default)
            => await TokenEncoder.ToContinuationEnumerable(
                GetFilteredEnumeration(query),
                query);

        /// <summary>
        /// Returns the filtered items. from the storage, before pagination.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The filtered items.</returns>
        protected abstract IEnumerable<TEntity> GetFilteredEnumeration(TQuery query);
    }
}
