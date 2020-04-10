using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Logging;
using Pantry.Queries;

namespace Pantry.InMemory
{
    /// <summary>
    /// <see cref="IRepository{T}"/> implementation using in-memory storage.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class ConcurrentDictionaryRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionaryRepository{T}"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="idGenerator">The <see cref="IIdGenerator{T}"/>.</param>
        /// <param name="etagGenerator">The <see cref="IETagGenerator{T}"/>.</param>
        /// <param name="continuationTokenEncoder">The <see cref="IContinuationTokenEncoder{TContinuationToken}"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ConcurrentDictionaryRepository(
            ConcurrentDictionary<string, TEntity> storage,
            IIdGenerator<TEntity> idGenerator,
            IETagGenerator<TEntity> etagGenerator,
            IContinuationTokenEncoder<LimitOffsetContinuationToken> continuationTokenEncoder,
            ILogger<ConcurrentDictionaryRepository<TEntity>>? logger = null)
        {
            Storage = storage;
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            EtagGenerator = etagGenerator ?? throw new ArgumentNullException(nameof(etagGenerator));
            ContinuationTokenEncoder = continuationTokenEncoder ?? throw new ArgumentNullException(nameof(continuationTokenEncoder));
            Logger = logger ?? NullLogger<ConcurrentDictionaryRepository<TEntity>>.Instance;
        }

        /// <summary>
        /// Gets the storage.
        /// </summary>
        protected ConcurrentDictionary<string, TEntity> Storage { get; }

        /// <summary>
        /// Gets the <see cref="IIdGenerator{T}"/>.
        /// </summary>
        protected IIdGenerator<TEntity> IdGenerator { get; }

        /// <summary>
        /// Gets the <see cref="IETagGenerator{T}"/>.
        /// </summary>
        protected IETagGenerator<TEntity> EtagGenerator { get; }

        /// <summary>
        /// Gets the <see cref="IContinuationTokenEncoder{LimitOffsetContinuationToken}"/>.
        /// </summary>
        protected IContinuationTokenEncoder<LimitOffsetContinuationToken> ContinuationTokenEncoder { get; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc/>
        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = await IdGenerator.Generate(entity);
            }

            if (entity is IETaggable taggableEntity && string.IsNullOrEmpty(taggableEntity.ETag))
            {
                taggableEntity.ETag = await EtagGenerator.Generate(entity);
            }

            if (Storage.TryAdd(entity.Id, entity))
            {
                Logger.LogAdded(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    entity: entity);

                return entity;
            }

            var conflictException = new ConflictException(typeof(TEntity).Name, entity.Id);
            Logger.LogAddedWarning(
                entityType: typeof(TEntity),
                entityId: entity.Id,
                warning: "Conflict",
                exception: conflictException);
            throw conflictException;
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            Storage.TryGetValue(id, out var result);

            Logger.LogGetById(
                entityType: typeof(TEntity),
                entityId: id,
                entity: result);

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var existing = await TryGetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            if (existing is null)
            {
                var notFoundEx = new NotFoundException(typeof(TEntity).Name, entity.Id);
                Logger.LogUpdatedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "NotFound",
                    exception: notFoundEx);

                throw notFoundEx;
            }

            if (existing is IETaggable existingTaggableEntity
                && entity is IETaggable taggableEntity
                && !string.IsNullOrEmpty(taggableEntity.ETag)
                && !string.IsNullOrEmpty(existingTaggableEntity.ETag))
            {
                if (existingTaggableEntity.ETag != taggableEntity.ETag)
                {
                    var concurrencyEx = new ConcurrencyException(
                        typeof(TEntity).Name,
                        entity.Id,
                        $"Mismatched ETag for {entity}: {taggableEntity.ETag} != {existingTaggableEntity.ETag}");
                    Logger.LogUpdatedWarning(
                        entityType: typeof(TEntity),
                        entityId: entity.Id,
                        warning: "Concurrency",
                        exception: concurrencyEx);

                    throw concurrencyEx;
                }
            }

            if (entity is IETaggable taggableEntityUpdate)
            {
                taggableEntityUpdate.ETag = await EtagGenerator.Generate(entity);
            }

            if (Storage.TryUpdate(entity.Id, entity, existing))
            {
                Logger.LogUpdated(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    entity: entity);
                return entity;
            }

            var concurrencyEx2 = new ConcurrencyException(typeof(TEntity).Name, entity.Id);
            Logger.LogUpdatedWarning(
                entityType: typeof(TEntity),
                entityId: entity.Id,
                warning: "Concurrency",
                exception: concurrencyEx2);

            throw concurrencyEx2;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            if (Storage.TryRemove(id, out var _))
            {
                Logger.LogDeleted(
                    entityType: typeof(TEntity),
                    entityId: id);

                return true;
            }

            Logger.LogDeletedWarning(
                entityType: typeof(TEntity),
                entityId: id,
                warning: "NotFound");
            return false;
        }

        /// <inheritdoc/>
        public async Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = Query.DefaultLimit, CancellationToken cancellationToken = default)
        {
            var result = await ContinuationTokenEncoder.ToContinuationEnumerable(
                Storage.Values,
                continuationToken,
                limit);
            Logger.LogFind($"(ct: {continuationToken ?? "<no-ct>"}, limit: {limit})", result);
            return result;
        }
    }
}
