using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Pantry.Continuation;
using Pantry.Queries;

namespace Pantry.InMemory.Queries
{
    /// <summary>
    /// Handles <see cref="MirrorQuery{TResult}"/> for <see cref="ConcurrentDictionaryRepository{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The repository entity type.</typeparam>
    public class ConcurrentDictionaryMirrorQueryHandler<TEntity> : ConcurrentDictionaryQueryHandler<TEntity, MirrorQuery<TEntity>>
        where TEntity : class, IIdentifiable, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionaryMirrorQueryHandler{TEntity}"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="tokenEncoder">The continuation token encoder.</param>
        public ConcurrentDictionaryMirrorQueryHandler(
            ConcurrentDictionary<string, TEntity> storage,
            IContinuationTokenEncoder<LimitOffsetContinuationToken> tokenEncoder)
            : base(storage, tokenEncoder)
        {
        }

        /// <inheritdoc/>
        protected override IEnumerable<TEntity> GetFilteredEnumeration(MirrorQuery<TEntity> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var enumerable = Storage.Values.AsEnumerable();

            if (!string.IsNullOrEmpty(query.Mirror.Id))
            {
                enumerable = enumerable.Where(x => x.Id == query.Mirror.Id);
            }

            foreach (var property in typeof(TEntity).GetProperties().Where(x => x.Name != nameof(IIdentifiable.Id)))
            {
                var value = property.GetValue(query.Mirror);
                if (value != null)
                {
                    var defaultValueType = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;
                    if (value != defaultValueType)
                    {
                        enumerable = enumerable.Where(x => property.GetValue(x) == value);
                    }
                }
            }

            return enumerable;
        }
    }
}
