using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public ConcurrentDictionaryMirrorQueryHandler(
            ConcurrentDictionary<string, TEntity> storage)
            : base(storage)
        {
        }

        /// <inheritdoc/>
        public override async Task<IContinuationEnumerable<TEntity>> ExecuteAsync(MirrorQuery<TEntity> query, CancellationToken cancellationToken = default)
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

            return enumerable.ToContinuationEnumerable(query);
        }
    }
}
