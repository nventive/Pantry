using System;
using System.Collections.Generic;

namespace Pantry.Tests.StandardTestSupport
{
    /// <summary>
    /// Using the dispose pattern, will add entities to the repository, and then removes them.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class TemporaryEntitiesScope<TEntity> : IDisposable
        where TEntity : class, IIdentifiable, IETaggable
    {
        private readonly ICrudRepository<TEntity> _repository;
        private readonly IEnumerable<TEntity> _entitySet;

        public TemporaryEntitiesScope(
            ICrudRepository<TEntity> repository,
            IEnumerable<TEntity> entitySet)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _entitySet = entitySet ?? throw new ArgumentNullException(nameof(entitySet));

            foreach (var entity in _entitySet)
            {
                var result = _repository.AddAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
                entity.Id = result.Id;
                entity.ETag = result.ETag;
            }
        }

        public TemporaryEntitiesScope(
            ICrudRepository<TEntity> repository,
            TEntity entity)
            : this(repository, new[] { entity })
        {
        }

        public void Dispose()
        {
            foreach (var entity in _entitySet)
            {
                _repository.TryRemoveAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}
