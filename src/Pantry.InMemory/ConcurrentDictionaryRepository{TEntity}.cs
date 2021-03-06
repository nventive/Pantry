﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Generators;
using Pantry.InMemory.Queries;
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
    public class ConcurrentDictionaryRepository<TEntity> : Repository<TEntity>,
                                                           IRepositoryClear<TEntity>,
                                                           IRepositoryFind<TEntity, TEntity, InMemoryLinqQuery<TEntity>>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionaryRepository{T}"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public ConcurrentDictionaryRepository(
            ConcurrentDictionary<string, TEntity> storage,
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// Gets the storage.
        /// </summary>
        protected ConcurrentDictionary<string, TEntity> Storage { get; }

        /// <summary>
        /// Gets the <see cref="IContinuationTokenEncoder{LimitOffsetContinuationToken}"/>.
        /// </summary>
        protected IContinuationTokenEncoder<LimitOffsetContinuationToken> ContinuationTokenEncoder => ServiceProvider.GetRequiredService<IContinuationTokenEncoder<LimitOffsetContinuationToken>>();

        /// <inheritdoc/>
        public override async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
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
        public override async Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            Storage.TryGetValue(id, out var result);

            Logger.LogGetById(
                entityType: typeof(TEntity),
                entityId: id,
                entity: result);

            return result;
        }

        /// <inheritdoc/>
        public override async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
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
        public override async Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default)
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
        public override async Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = RepositoryQuery.DefaultLimit, CancellationToken cancellationToken = default)
        {
            var result = await ContinuationTokenEncoder.ToContinuationEnumerable(
                Storage.Values,
                continuationToken,
                limit);
            Logger.LogFind($"(ct: {continuationToken ?? "<no-ct>"}, limit: {limit})", result);
            return result;
        }

        /// <inheritdoc/>
        public override async Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaRepositoryQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var queryable = Storage.Values.AsQueryable();

            foreach (var criterion in query.GetCriterions())
            {
                if (criterion is PropertyCriterion propertyCriterion && propertyCriterion.PropertyPathContainsIndexer)
                {
                    throw new UnsupportedFeatureException($"{GetType().Name} does not support property indexers ({propertyCriterion.PropertyPath}).");
                }

                queryable = criterion switch
                {
                    EqualToPropertyCriterion equalTo => queryable.Where($"{equalTo.PropertyPath} == @0", equalTo.Value),
                    NotEqualToPropertyCriterion notEqualTo => queryable.Where($"{notEqualTo.PropertyPath} != @0", notEqualTo.Value),
                    NullPropertyCriterion nullCrit => queryable.Where($"{nullCrit.PropertyPath} {(nullCrit.IsNull ? "==" : "!=")} null"),
                    GreaterThanPropertyCriterion gt => queryable.Where($"{gt.PropertyPath} > @0", gt.Value),
                    GreaterThanOrEqualToPropertyCriterion gte => queryable.Where($"{gte.PropertyPath} >= @0", gte.Value),
                    LessThanPropertyCriterion lt => queryable.Where($"{lt.PropertyPath} < @0", lt.Value),
                    LessThanOrEqualToPropertyCriterion lte => queryable.Where($"{lte.PropertyPath} <= @0", lte.Value),
                    StringContainsPropertyCriterion strCont => queryable.Where($"{strCont.PropertyPath}.Contains(@0)", strCont.Value),
                    InPropertyCriterion inProp => inProp.Values != null && inProp.Values.Any()
                        ? queryable.Where(
                            string.Join(
                                " || ",
                                inProp.Values.Select((val, i) => $"({inProp.PropertyPath} == @{i})")),
                            inProp.Values.ToArray())
                        : queryable,
                    NotInPropertyCriterion notInProp => notInProp.Values != null && notInProp.Values.Any()
                    ? queryable.Where(
                        "!(" +
                        string.Join(
                            " && ",
                            notInProp.Values.Select((val, i) => $"({notInProp.PropertyPath} == @{i})")) +
                        ")",
                        notInProp.Values.ToArray())
                    : queryable,
                    OrderCriterion order => queryable.OrderBy($"{order.PropertyPath} {(order.Ascending ? "ASC" : "DESC")}"),
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
        public virtual Task ClearAsync(CancellationToken cancellationToken = default)
        {
            Storage.Clear();
            Logger.LogClear(typeof(TEntity));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual async Task<IContinuationEnumerable<TEntity>> FindAsync(
            InMemoryLinqQuery<TEntity> query,
            CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var queryable = Storage.Values.AsQueryable();
            queryable = query.Apply(queryable);

            var result = await ContinuationTokenEncoder.ToContinuationEnumerable(
                queryable.ToList(),
                query.ContinuationToken,
                query.Limit);
            Logger.LogFind(query, result);
            return result;
        }
    }
}
