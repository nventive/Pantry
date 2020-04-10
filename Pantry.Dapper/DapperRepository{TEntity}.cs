using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Generators;
using Pantry.Logging;
using Pantry.Providers;

namespace Pantry.Dapper
{
    /// <summary>
    /// Dapper Repository Implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class DapperRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IIdentifiable
    {
        private DapperRepositoryDatabase<TEntity>? _dapperRepositoryDatabase;

        /// <summary>
        /// Initializes a new instance of the <see cref="DapperRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="dbConnectionFor">The <see cref="DbConnectionFor"/>.</param>
        /// <param name="idGenerator">The <see cref="IIdGenerator{T}"/>.</param>
        /// <param name="etagGenerator">The <see cref="IETagGenerator{T}"/>.</param>
        /// <param name="timestampProvider">The <see cref="ITimestampProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public DapperRepository(
            DbConnectionFor<TEntity> dbConnectionFor,
            IIdGenerator<TEntity> idGenerator,
            IETagGenerator<TEntity> etagGenerator,
            ITimestampProvider timestampProvider,
            ILogger<DapperRepository<TEntity>>? logger = null)
        {
            DbConnectionFor = dbConnectionFor ?? throw new ArgumentNullException(nameof(dbConnectionFor));
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            EtagGenerator = etagGenerator ?? throw new ArgumentNullException(nameof(etagGenerator));
            TimestampProvider = timestampProvider ?? throw new ArgumentNullException(nameof(timestampProvider));
            Logger = logger ?? NullLogger<DapperRepository<TEntity>>.Instance;
        }

        /// <summary>
        /// Gets the <see cref="DbConnectionFor"/>.
        /// </summary>
        protected DbConnectionFor<TEntity> DbConnectionFor { get; }

        /// <summary>
        /// Gets the <see cref="DbConnection"/>.
        /// </summary>
        protected DbConnection DbConnection => DbConnectionFor.DbConnection;

        /// <summary>
        /// Gets the Dapper Rainbow Database.
        /// </summary>
        protected DapperRepositoryDatabase<TEntity> Database => _dapperRepositoryDatabase ??= DapperRepositoryDatabase<TEntity>.Init(DbConnection, commandTimeout: 3);

        /// <summary>
        /// Gets the Dapper Rainbow Table.
        /// </summary>
        protected DapperRepositoryDatabase<TEntity>.Table<TEntity, string> Table => Database.Table!;

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

            await Table.InsertAsync(entity).ConfigureAwait(false);

            Logger.LogAdded(
                entityType: typeof(TEntity),
                entityId: entity.Id,
                entity: entity);

            return entity;
        }

        /// <inheritdoc/>
        public virtual Task<TEntity> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
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

            if (await Table.DeleteAsync(id).ConfigureAwait(false))
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
        public virtual Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = 50, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }
    }
}
