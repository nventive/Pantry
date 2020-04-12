using System;
using System.Collections.Generic;
using System.Linq;
using Pantry.Traits;

namespace Pantry.Tests.StandardTestSupport
{
    /// <summary>
    /// Using the dispose pattern, will add entities to the repository, and then removes them.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class TemporaryEntitiesScope<TEntity> : IDisposable
        where TEntity : class, IIdentifiable, IETaggable, ITimestamped
    {
        private readonly object _repository;
        private readonly IEnumerable<TEntity> _entitySet;

        public TemporaryEntitiesScope(
            object repository,
            IEnumerable<TEntity> entitySet,
            bool cleanUpOnly = false)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _entitySet = entitySet ?? throw new ArgumentNullException(nameof(entitySet));

            if (!cleanUpOnly && _repository is IRepositoryAdd<TEntity> repoAdd)
            {
                foreach (var entity in _entitySet)
                {
                    var result = repoAdd.AddAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
                    entity.Id = result.Id;
                    entity.ETag = result.ETag;
                    entity.Timestamp = result.Timestamp;
                }
            }
        }

        public TemporaryEntitiesScope(
            object repository,
            TEntity entity,
            bool cleanUpOnly = false)
            : this(repository, new[] { entity }, cleanUpOnly)
        {
        }

        public void Dispose()
        {
            if (_repository is IRepositoryRemove<TEntity> repoRemove)
            {
                foreach (var entity in _entitySet.Where(x => !string.IsNullOrEmpty(x.Id)))
                {
                    repoRemove.TryRemoveAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }
    }
}
