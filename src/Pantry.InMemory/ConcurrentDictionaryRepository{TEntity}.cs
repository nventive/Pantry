﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Generators;
using Pantry.Logging;
using Pantry.Providers;
using Pantry.Queries;
using Pantry.Queries.Criteria;
using Pantry.Traits;

namespace Pantry.InMemory
{
    /// <summary>
    /// <see cref="IRepository{T}"/> implementation using in-memory storage.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class ConcurrentDictionaryRepository<TEntity> : IRepository<TEntity>, IRepositoryClear<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionaryRepository{T}"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="idGenerator">The <see cref="IIdGenerator{T}"/>.</param>
        /// <param name="etagGenerator">The <see cref="IETagGenerator{T}"/>.</param>
        /// <param name="timestampProvider">The <see cref="ITimestampProvider"/>.</param>
        /// <param name="continuationTokenEncoder">The <see cref="IContinuationTokenEncoder{TContinuationToken}"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ConcurrentDictionaryRepository(
            ConcurrentDictionary<string, TEntity> storage,
            IIdGenerator<TEntity> idGenerator,
            IETagGenerator<TEntity> etagGenerator,
            ITimestampProvider timestampProvider,
            IContinuationTokenEncoder<LimitOffsetContinuationToken> continuationTokenEncoder,
            ILogger<ConcurrentDictionaryRepository<TEntity>>? logger = null)
        {
            Storage = storage;
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            EtagGenerator = etagGenerator ?? throw new ArgumentNullException(nameof(etagGenerator));
            TimestampProvider = timestampProvider ?? throw new ArgumentNullException(nameof(timestampProvider));
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
        /// Gets the <see cref="ITimestampProvider"/>.
        /// </summary>
        protected ITimestampProvider TimestampProvider { get; }

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

            if (entity is ITimestamped timestampedEntity && timestampedEntity.Timestamp is null)
            {
                timestampedEntity.Timestamp = TimestampProvider.CurrentTimestamp();
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
        public virtual async Task<TEntity> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var existingEntity = await TryGetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            if (existingEntity is null)
            {
                return await AddAsync(entity, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            }
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

            if (entity is ITimestamped timestampedEntity && timestampedEntity.Timestamp is null)
            {
                timestampedEntity.Timestamp = TimestampProvider.CurrentTimestamp();
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
            if (string.IsNullOrEmpty(id))
            {
                Logger.LogDeletedWarning(
                    entityType: typeof(TEntity),
                    entityId: "(null)",
                    warning: "NotFound");

                return false;
            }

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

        /// <inheritdoc/>
        public async Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var queryable = Storage.Values.AsQueryable();

            foreach (var criterion in query)
            {
                if (criterion is PropertyCriterion propertyCriterion && propertyCriterion.PropertyPathContainsSubPath)
                {
                    throw new UnsupportedFeatureException($"{GetType().Name} does not support sub-property selection ({propertyCriterion.PropertyPath}).");
                }

                queryable = criterion switch
                {
                    IQueryableCriterion queryableCriterion => (IQueryable<TEntity>)queryableCriterion.Apply(queryable),
                    _ => throw new UnsupportedFeatureException($"The {criterion} criterion is not supported by {this}."),
                };
            }

            var result = await ContinuationTokenEncoder.ToContinuationEnumerable(
                queryable.ToList(),
                query.ContinuationToken,
                query.Limit);
            Logger.LogFind(query, result);
            return result;
        }

        /// <inheritdoc/>
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            Storage.Clear();
            Logger.LogClear(typeof(TEntity));
            return Task.CompletedTask;
        }
    }
}