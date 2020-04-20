﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Generators;
using Pantry.Logging;
using Pantry.Providers;
using Pantry.Queries;
using Pantry.Traits;
using PetaPoco;

namespace Pantry.PetaPoco
{
    /// <summary>
    /// PetaPoco Repository Implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class PetaPocoRepository<TEntity> : IRepository<TEntity>,
                                               IRepositoryClear<TEntity>,
                                               IHealthCheck
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PetaPocoRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="databaseFor">The <see cref="PetaPocoDatabaseFor{TEntity}"/>.</param>
        /// <param name="idGenerator">The <see cref="IIdGenerator{TEntity}"/>.</param>
        /// <param name="etagGenerator">The <see cref="IETagGenerator{T}"/>.</param>
        /// <param name="timestampProvider">The <see cref="ITimestampProvider"/>.</param>
        /// <param name="continuationTokenEncoder">The <see cref="IContinuationTokenEncoder{TContinuationToken}"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public PetaPocoRepository(
            PetaPocoDatabaseFor<TEntity> databaseFor,
            IIdGenerator<TEntity> idGenerator,
            IETagGenerator<TEntity> etagGenerator,
            ITimestampProvider timestampProvider,
            IContinuationTokenEncoder<LimitPageContinuationToken> continuationTokenEncoder,
            ILogger<PetaPocoRepository<TEntity>>? logger = null)
        {
            DatabaseFor = databaseFor ?? throw new ArgumentNullException(nameof(databaseFor));
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            EtagGenerator = etagGenerator ?? throw new ArgumentNullException(nameof(etagGenerator));
            TimestampProvider = timestampProvider ?? throw new ArgumentNullException(nameof(timestampProvider));
            ContinuationTokenEncoder = continuationTokenEncoder ?? throw new ArgumentNullException(nameof(continuationTokenEncoder));
            Logger = logger ?? NullLogger<PetaPocoRepository<TEntity>>.Instance;

            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Database.CommandExecuting += (_, args) =>
                {
                    Logger.LogTrace(
                        "[CommandExecuting] : {CommandText} {CommandParameters}",
                        args.Command.CommandText,
                        string.Join(", ", args.Command.Parameters.Cast<DbParameter>().Select(x => $"{x.ParameterName}={x.Value}")));
                };
            }
        }

        /// <summary>
        /// Gets the <see cref="PetaPocoDatabaseFor{TEntity}"/>.
        /// </summary>
        protected PetaPocoDatabaseFor<TEntity> DatabaseFor { get; }

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
        /// Gets the <see cref="IContinuationTokenEncoder{LimitPageContinuationToken}"/>.
        /// </summary>
        protected IContinuationTokenEncoder<LimitPageContinuationToken> ContinuationTokenEncoder { get; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the <see cref="IDatabase"/>.
        /// </summary>
        protected IDatabase Database => DatabaseFor.Database;

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

            try
            {
                await Database.InsertAsync(cancellationToken, entity).ConfigureAwait(false);

                Logger.LogAdded(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    entity: entity);

                return entity;
            }
            catch (DbException dbEx)
            {
                // This is probably the simplest way to check for conflict errors cross-database provider :-(
                var exists = await Database.SingleOrDefaultAsync<TEntity>(cancellationToken, (object)entity.Id).ConfigureAwait(false);
                if (exists != null)
                {
                    var conflictException = new ConflictException(typeof(TEntity).Name, entity.Id, dbEx);
                    Logger.LogAddedWarning(
                        entityType: typeof(TEntity),
                        entityId: entity.Id,
                        warning: "Conflict",
                        exception: conflictException);
                    throw conflictException;
                }

                throw;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<(TEntity, bool)> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.Id))
            {
                return (await AddAsync(entity, cancellationToken).ConfigureAwait(false), true);
            }

            var existingEntity = await TryGetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            if (existingEntity is null)
            {
                return (await AddAsync(entity, cancellationToken).ConfigureAwait(false), true);
            }
            else
            {
                return (await UpdateAsync(entity, cancellationToken).ConfigureAwait(false), false);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var result = await Database.SingleOrDefaultAsync<TEntity>(cancellationToken, (object)id).ConfigureAwait(false);

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

            string? entryETag = null;
            if (entity is IETaggable taggableEntity)
            {
                entryETag = taggableEntity.ETag;
                taggableEntity.ETag = await EtagGenerator.Generate(entity);
            }

            if (entity is ITimestamped timestampedEntity && timestampedEntity.Timestamp is null)
            {
                timestampedEntity.Timestamp = TimestampProvider.CurrentTimestamp();
            }

            if (!string.IsNullOrEmpty(entryETag))
            {
                // This might get better with PetaPoco 6.5 with support for Version - optimistic concurrency).
                try
                {
                    await Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                    var currentEntityVersion = await Database.SingleOrDefaultAsync<TEntity>((object)entity.Id).ConfigureAwait(false);
                    if (currentEntityVersion is null)
                    {
                        var exception = new NotFoundException(typeof(TEntity).Name, entity.Id);
                        Logger.LogUpdatedWarning(
                            entityType: typeof(TEntity),
                            entityId: entity.Id,
                            warning: "NotFound",
                            exception: exception);

                        throw exception;
                    }

                    if (((IETaggable)currentEntityVersion).ETag != entryETag)
                    {
                        var exception = new ConcurrencyException(typeof(TEntity).Name, entity.Id);
                        Logger.LogUpdatedWarning(
                            entityType: typeof(TEntity),
                            entityId: entity.Id,
                            warning: "Concurrency",
                            exception: exception);

                        throw exception;
                    }

                    await Database.UpdateAsync(cancellationToken, entity).ConfigureAwait(false);
                    Database.CompleteTransaction();

                    return entity;
                }
                catch (Exception)
                {
                    Database.AbortTransaction();
                    throw;
                }
            }
            else
            {
                var numberOfRecordsAffected = await Database.UpdateAsync(cancellationToken, entity).ConfigureAwait(false);

                if (numberOfRecordsAffected == 0)
                {
                    var exception = new NotFoundException(typeof(TEntity).Name, entity.Id);
                    Logger.LogUpdatedWarning(
                        entityType: typeof(TEntity),
                        entityId: entity.Id,
                        warning: "NotFound",
                        exception: exception);

                    throw exception;
                }

                Logger.LogUpdated(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    entity: entity);

                return entity;
            }
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

            var result = await Database.DeleteAsync<TEntity>(cancellationToken, (object)id).ConfigureAwait(false);
            if (result != 1)
            {
                Logger.LogDeletedWarning(
                    entityType: typeof(TEntity),
                    entityId: id,
                    warning: "NotFound");
                return false;
            }

            Logger.LogDeleted(
                entityType: typeof(TEntity),
                entityId: id);

            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = Query.DefaultLimit, CancellationToken cancellationToken = default)
        {
            if (limit <= 0)
            {
                return ContinuationEnumerable.Empty<TEntity>();
            }

            var pagination = await ContinuationTokenEncoder.Decode(continuationToken);
            if (pagination is null)
            {
                pagination = new LimitPageContinuationToken { Limit = limit, Page = 0 };
            }

            var pagedItems = await Database.PageAsync<TEntity>(cancellationToken, pagination.Page, pagination.Limit).ConfigureAwait(false);

            var result = pagedItems.Items.ToContinuationEnumerable(
                pagedItems.TotalPages <= pagination.Page
                    ? null
                    : await ContinuationTokenEncoder.Encode(
                        new LimitPageContinuationToken
                        {
                            Page = Convert.ToInt32(pagedItems.CurrentPage + 1),
                            Limit = pagination.Limit,
                        }));

            Logger.LogFind($"(ct: {continuationToken ?? "<no-ct>"}, limit: {limit})", result);

            return result;
        }

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>
            {
                { nameof(Database.Provider), Database.Provider.ToString() },
            };

            try
            {
                await Database.FirstOrDefaultAsync<TEntity>(cancellationToken, string.Empty).ConfigureAwait(false);
                return HealthCheckResult.Healthy(data: data);
            }
            catch (DbException ex)
            {
                Logger.LogError(ex, "An exception occured during the heatlh check: {Message}", ex.Message);
                return HealthCheckResult.Unhealthy(
                    description: $"A {nameof(DbException)} occured during the check.",
                    exception: ex,
                    data: data);
            }
        }

        /// <inheritdoc/>
        public virtual async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await Database.DeleteAsync<TEntity>(cancellationToken, string.Empty).ConfigureAwait(false);
            Logger.LogClear(typeof(TEntity));
        }
    }
}
