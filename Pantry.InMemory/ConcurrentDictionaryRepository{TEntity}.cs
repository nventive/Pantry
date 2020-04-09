using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.InMemory.Queries;
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
        /// <param name="queryHandlerExecutor">The query handler executor.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ConcurrentDictionaryRepository(
            ConcurrentDictionary<string, TEntity> storage,
            IIdGenerator<TEntity> idGenerator,
            IQueryHandlerExecutor<TEntity, IConcurrentDictionaryQueryHandler> queryHandlerExecutor,
            ILogger<ConcurrentDictionaryRepository<TEntity>>? logger = null)
        {
            Storage = storage;
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            QueryHandlerExecutor = queryHandlerExecutor;
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
        /// Gets the <see cref="IQueryHandlerExecutor{TEntity, IConcurrentDictionaryQueryHandler}"/>.
        /// </summary>
        protected IQueryHandlerExecutor<TEntity, IConcurrentDictionaryQueryHandler> QueryHandlerExecutor { get; }

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

            if (Storage.TryUpdate(entity.Id, entity, existing))
            {
                Logger.LogUpdated(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    entity: entity);
                return entity;
            }

            var concurrencyEx = new ConcurrencyException(typeof(TEntity).Name, entity.Id);
            Logger.LogUpdatedWarning(
                entityType: typeof(TEntity),
                entityId: entity.Id,
                warning: "Concurrency",
                exception: concurrencyEx);

            throw concurrencyEx;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryDeleteAsync(string id, CancellationToken cancellationToken = default)
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
        public virtual Task<IContinuationEnumerable<TResult>> FindAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return QueryHandlerExecutor.ExecuteAsync<TResult, IQuery<TResult>>(query);
        }
    }
}
