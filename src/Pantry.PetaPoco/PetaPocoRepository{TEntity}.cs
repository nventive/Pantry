using System;
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
using Pantry.PetaPoco.Queries;
using Pantry.Providers;
using Pantry.Queries;
using Pantry.Queries.Criteria;
using Pantry.Traits;
using PetaPoco;
using PetaPoco.SqlKata;

namespace Pantry.PetaPoco
{
    /// <summary>
    /// PetaPoco Repository Implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class PetaPocoRepository<TEntity> : IRepository<TEntity>,
                                               IRepositoryFind<TEntity, TEntity, PetaPocoSqlBuilderQuery<TEntity>>,
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
            IContinuationTokenEncoder<PageContinuationToken> continuationTokenEncoder,
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
        protected IContinuationTokenEncoder<PageContinuationToken> ContinuationTokenEncoder { get; }

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
        public virtual Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = Query.DefaultLimit, CancellationToken cancellationToken = default)
        {
            return ExecuteFindAsync(
                new FindAllQuery<TEntity> { ContinuationToken = continuationToken, Limit = limit },
                _ => { },
                cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            // Thanks PGSQL for being so complex / so different with JSON...:-(
            static string JsonPathForPostgres(string[] columnPath, object? comparingValue)
            {
                var columnName = columnPath[0];
                columnPath = columnPath.Skip(1).Select(x => $"'{x}'").ToArray();
                var jsonExpression = columnName + string.Join("-­>", columnPath, 0, columnPath.Length - 1) + "->>" + columnPath.LastOrDefault();
                switch (comparingValue)
                {
                    case int _:
                    case IEnumerable<int> e_:
                        return $"CAST({jsonExpression} AS INTEGER)";
                    case double _:
                    case IEnumerable<double> e_:
                        return $"CAST({jsonExpression} AS FLOAT8)";
                    case bool _:
                    case IEnumerable<bool> e_:
                        return $"CAST({jsonExpression} AS BOOLEAN)";
                    case DateTime _:
                    case IEnumerable<DateTime> e_:
                        return $"CAST({jsonExpression} AS TIMESTAMP)";
                    case DateTimeOffset _:
                    case IEnumerable<DateTimeOffset> e_:
                        return $"CAST({jsonExpression} AS TIMESTAMPZ)";
                    case decimal _:
                    case IEnumerable<decimal> e_:
                        return $"CAST({jsonExpression} AS DECIMAL(19,5))";
                    case Guid _:
                    case IEnumerable<Guid> e_:
                        return $"CAST({jsonExpression} AS UUID)";
                    default:
                        return jsonExpression;
                }
            }

            return await ExecuteFindAsync(
                query,
                (queryBuilder) =>
                {
                    foreach (var criterion in query.GetCriterions())
                    {
                        if (criterion is PropertyCriterion propertyCriterion && propertyCriterion.PropertyPathContainsIndexer)
                        {
                            throw new UnsupportedFeatureException($"{GetType().Name} does not support property indexers ({propertyCriterion.PropertyPath}).");
                        }

                        var column = criterion is PropertyCriterion propertyPathCriterion ? propertyPathCriterion.PropertyPath : string.Empty;
                        if (column.Contains(".", StringComparison.Ordinal))
                        {
                            var columnPath = column.Split(".");
                            var columnName = columnPath[0];
                            var remainingPath = string.Join(".", columnPath.Skip(1));
                            // This is where we enter an untested/unreliable portion. There be dragons.
                            column = GetSqlKataCompilerType() switch
                            {
                                CompilerType.SqlServer => $"JSON_VALUE({columnName}, '$.{remainingPath}')",
                                CompilerType.SQLite => $"json_extract({columnName}, '$.{remainingPath}')",
                                CompilerType.MySql => $"JSON_UNQUOTE(JSON_EXTRACT({columnName}, '$.{remainingPath}'))",
                                CompilerType.Postgres => JsonPathForPostgres(columnPath, criterion is PropertyValueCriterion pvc ? pvc.Value : null),
                                _ => throw new UnsupportedFeatureException($"{GetType().Name} does not support property selection with path ({column}) on {GetSqlKataCompilerType()}."),
                            };
                        }
                        else
                        {
                            column = Database.Provider.EscapeSqlIdentifier(column);
                        }

                        queryBuilder = criterion switch
                        {
                            EqualToPropertyCriterion equalTo => queryBuilder.WhereRaw($"{column} = ?", equalTo.Value),
                            NotEqualToPropertyCriterion notEqualTo => queryBuilder.WhereRaw($"{column} <> ?", notEqualTo.Value),
                            NullPropertyCriterion nullCrit => queryBuilder.WhereRaw($"{column} IS {(nullCrit.IsNull ? string.Empty : "NOT ")}NULL"),
                            GreaterThanPropertyCriterion gt => queryBuilder.WhereRaw($"{column} > ?", gt.Value),
                            GreaterThanOrEqualToPropertyCriterion gte => queryBuilder.WhereRaw($"{column} >= ?", gte.Value),
                            LessThanPropertyCriterion lt => queryBuilder.WhereRaw($"{column} < ?", lt.Value),
                            LessThanOrEqualToPropertyCriterion lte => queryBuilder.WhereRaw($"{column} <= ?", lte.Value),
                            StringContainsPropertyCriterion strCont => queryBuilder.WhereRaw($"{column} LIKE ?", $"%{strCont.Value}%"),
                            InPropertyCriterion inProp => queryBuilder.WhereRaw($"{column} IN (?)", inProp.Values.ToArray()),
                            NotInPropertyCriterion notInProp => queryBuilder.WhereRaw($"{column} NOT IN (?)", notInProp.Values.ToArray()),
                            OrderCriterion order => queryBuilder.OrderByRaw($"{column} {(order.Ascending ? "ASC" : "DESC")}"),
                            _ => throw new UnsupportedFeatureException($"The {criterion} criterion is not supported by {this}."),
                        };
                    }
                },
                cancellationToken).ConfigureAwait(false);
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
        public virtual Task<IContinuationEnumerable<TEntity>> FindAsync(PetaPocoSqlBuilderQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            return ExecuteFindAsync(
                query,
                x => query.Apply(x),
                cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await Database.DeleteAsync<TEntity>(cancellationToken, string.Empty).ConfigureAwait(false);
            Logger.LogClear(typeof(TEntity));
        }

        /// <summary>
        /// Executes a query with proper pagination.
        /// Prepares the SqlKata query with the proper select statement.
        /// </summary>
        /// <param name="query">The <see cref="IQuery{TResult}"/>.</param>
        /// <param name="queryBuilder">Builds additional criterias.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="IContinuationEnumerable{TEntity}"/> with continuation token set.</returns>
        protected virtual Task<IContinuationEnumerable<TEntity>> ExecuteFindAsync(
            IQuery<TEntity> query,
            Action<SqlKata.Query> queryBuilder,
            CancellationToken cancellationToken)
        {
            if (queryBuilder is null)
            {
                throw new ArgumentNullException(nameof(queryBuilder));
            }

            var sqlQuery = new SqlKata.Query().GenerateSelect<TEntity>(GetDatabaseMapperForTheEntity());
            queryBuilder(sqlQuery);

            return ExecuteFindAsync(query, sqlQuery, cancellationToken);
        }

        /// <summary>
        /// Executes a query with proper pagination.
        /// </summary>
        /// <param name="query">The <see cref="IQuery{TResult}"/>.</param>
        /// <param name="sqlQuery">The SQLKata query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="IContinuationEnumerable{TEntity}"/> with continuation token set.</returns>
        protected virtual async Task<IContinuationEnumerable<TEntity>> ExecuteFindAsync(
            IQuery<TEntity> query,
            SqlKata.Query sqlQuery,
            CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (query.Limit <= 0)
            {
                return ContinuationEnumerable.Empty<TEntity>();
            }

            var pagination = await ContinuationTokenEncoder.Decode(query.ContinuationToken);
            if (pagination is null)
            {
                pagination = new PageContinuationToken { PerPage = query.Limit, Page = 1 };
            }

            var pagedItems = await Database.PageAsync<TEntity>(cancellationToken, pagination.Page, pagination.PerPage, sqlQuery.ToSql(GetSqlKataCompilerType())).ConfigureAwait(false);

            var result = pagedItems.Items.ToContinuationEnumerable(
                pagedItems.TotalPages <= pagination.Page
                    ? null
                    : await ContinuationTokenEncoder.Encode(
                        new PageContinuationToken
                        {
                            Page = Convert.ToInt32(pagedItems.CurrentPage + 1),
                            PerPage = pagination.PerPage,
                        }));

            Logger.LogFind(query, result);

            return result;
        }

        /// <summary>
        /// Gets the actual registered mapper for <typeparamref name="TEntity"/>.
        /// </summary>
        /// <returns>The <see cref="IMapper"/>.</returns>
        protected virtual IMapper GetDatabaseMapperForTheEntity()
            => Mappers.GetMapper(typeof(TEntity), Database.DefaultMapper);

        /// <summary>
        /// Gets the SqlKata Compiler type from the current <see cref="Database"/>.
        /// </summary>
        /// <returns>The compiler type.</returns>
        protected virtual CompilerType GetSqlKataCompilerType()
            => Database.Provider switch
            {
                global::PetaPoco.Providers.FirebirdDbDatabaseProvider _ => CompilerType.Firebird,
                global::PetaPoco.Providers.MariaDbDatabaseProvider _ => CompilerType.MySql,
                global::PetaPoco.Providers.MsAccessDbDatabaseProvider _ => CompilerType.SqlServer,
                global::PetaPoco.Providers.MySqlDatabaseProvider _ => CompilerType.MySql,
                global::PetaPoco.Providers.OracleDatabaseProvider _ => CompilerType.Oracle,
                global::PetaPoco.Providers.PostgreSQLDatabaseProvider _ => CompilerType.Postgres,
                global::PetaPoco.Providers.SQLiteDatabaseProvider _ => CompilerType.SQLite,
                global::PetaPoco.Providers.SqlServerCEDatabaseProviders _ => CompilerType.SqlServer,
                global::PetaPoco.Providers.SqlServerDatabaseProvider _ => CompilerType.SqlServer,
                _ => throw new UnsupportedFeatureException($"Unable to select an appropriate Compiler Type for SqlKata given the PetaPoco provider {Database.Provider}"),
            };
    }
}
