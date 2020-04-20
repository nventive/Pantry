using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Azure.TableStorage.Queries;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Generators;
using Pantry.Logging;
using Pantry.Queries;
using Pantry.Queries.Criteria;
using Pantry.Traits;

namespace Pantry.Azure.TableStorage
{
    /// <summary>
    /// Azure Table Storage Repository Implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class AzureTableStorageRepository<TEntity> : IRepository<TEntity>,
                                                        IRepositoryFind<TEntity, TEntity, AzureTableStorageTableQuery<TEntity>>,
                                                        IHealthCheck
        where TEntity : class, IIdentifiable, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageRepository{T}"/> class.
        /// </summary>
        /// <param name="cloudTableFor">The <see cref="CloudTableFor{T}"/> instance to use.</param>
        /// <param name="idGenerator">The <see cref="IIdGenerator{T}"/>.</param>
        /// <param name="mapper">The mapper to <see cref="DynamicTableEntity"/>.</param>
        /// <param name="continuationTokenEncoder">The <see cref="IContinuationTokenEncoder{TableContinuationToken}"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public AzureTableStorageRepository(
            CloudTableFor<TEntity> cloudTableFor,
            IIdGenerator<TEntity> idGenerator,
            IDynamicTableEntityMapper<TEntity> mapper,
            IContinuationTokenEncoder<TableContinuationToken> continuationTokenEncoder,
            ILogger<AzureTableStorageRepository<TEntity>>? logger = null)
        {
            CloudTable = cloudTableFor?.CloudTable ?? throw new ArgumentNullException(nameof(cloudTableFor));
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            ContinuationTokenEncoder = continuationTokenEncoder ?? throw new ArgumentNullException(nameof(continuationTokenEncoder));
            Logger = logger ?? NullLogger<AzureTableStorageRepository<TEntity>>.Instance;
        }

        /// <summary>
        /// Gets the <see cref="CloudTable"/>.
        /// </summary>
        protected CloudTable CloudTable { get; }

        /// <summary>
        /// Gets the <see cref="IIdGenerator{T}"/>.
        /// </summary>
        protected IIdGenerator<TEntity> IdGenerator { get; }

        /// <summary>
        /// Gets the <see cref="IDynamicTableEntityMapper{TEntity}"/>.
        /// </summary>
        protected IDynamicTableEntityMapper<TEntity> Mapper { get; }

        /// <summary>
        /// Gets the <see cref="IContinuationTokenEncoder{TableContinuationToken}"/>.
        /// </summary>
        protected IContinuationTokenEncoder<TableContinuationToken> ContinuationTokenEncoder { get; }

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

            var tableEntity = Mapper.MapToDestination(entity);

            try
            {
                var operationResult = await CloudTable.ExecuteAsync(
                    TableOperation.Insert(tableEntity),
                    cancellationToken)
                    .ConfigureAwait(false);

                var result = Mapper.MapToSource((DynamicTableEntity)operationResult.Result);

                Logger.LogAdded(
                    entityType: typeof(TEntity),
                    entityId: result.Id,
                    entity: result);

                return result;
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == 409)
            {
                var conflictException = new ConflictException(typeof(TEntity).Name, entity.Id, storageException);
                Logger.LogAddedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "Conflict",
                    exception: conflictException);
                throw conflictException;
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
            var (partitionKey, rowKey) = Mapper.GetStorageKeys(id);
            var operationResult = await CloudTable.ExecuteAsync(
                TableOperation.Retrieve(partitionKey, rowKey),
                cancellationToken)
                .ConfigureAwait(false);

            var result = operationResult.HttpStatusCode == 404
                ? null
                : Mapper.MapToSource((DynamicTableEntity)operationResult.Result);
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

            string? sentEtag = null; // To properly catch ETag format exceptions.
            try
            {
                var targetUpdatedTableEntity = Mapper.MapToDestination(entity);
                sentEtag = targetUpdatedTableEntity.ETag;
                if (string.IsNullOrEmpty(targetUpdatedTableEntity.ETag))
                {
                    targetUpdatedTableEntity.ETag = "*";
                }

                var operationResult = await CloudTable.ExecuteAsync(
                    TableOperation.Replace(targetUpdatedTableEntity),
                    cancellationToken)
                    .ConfigureAwait(false);

                var dynamicTableEntity = (DynamicTableEntity)operationResult.Result;
                if (dynamicTableEntity.Timestamp == default)
                {
                    dynamicTableEntity.Timestamp = ParseETagForTimestamp(operationResult.Etag);
                }

                var result = Mapper.MapToSource(dynamicTableEntity);
                Logger.LogUpdated(
                    entityType: typeof(TEntity),
                    entityId: result.Id,
                    entity: result);
                return result;
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == 404)
            {
                var exception = new NotFoundException(typeof(TEntity).Name, entity.Id, storageException);
                Logger.LogUpdatedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "NotFound",
                    exception: exception);

                throw exception;
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == 412)
            {
                var exception = new ConcurrencyException(typeof(TEntity).Name, entity.Id, storageException.Message, storageException);
                Logger.LogUpdatedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "Concurrency",
                    exception: exception);

                throw exception;
            }
            catch (ArgumentException argumentException) when (argumentException.Message.Contains("ETag", StringComparison.InvariantCultureIgnoreCase))
            {
                var exception = new ConcurrencyException(typeof(TEntity).Name, entity.Id, argumentException.Message, argumentException);
                Logger.LogUpdatedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "Concurrency",
                    exception: exception);

                throw exception;
            }
            catch (StorageException storageException) when (storageException.Message.Contains(sentEtag, StringComparison.InvariantCultureIgnoreCase))
            {
                var exception = new ConcurrencyException(typeof(TEntity).Name, entity.Id, storageException.Message, storageException);
                Logger.LogUpdatedWarning(
                    entityType: typeof(TEntity),
                    entityId: entity.Id,
                    warning: "Concurrency",
                    exception: exception);

                throw exception;
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

            var (partitionKey, rowKey) = Mapper.GetStorageKeys(id);
            var result = await CloudTable.ExecuteAsync(
                TableOperation.Retrieve(partitionKey, rowKey),
                cancellationToken)
                .ConfigureAwait(false);

            if (result.HttpStatusCode == 404)
            {
                Logger.LogDeletedWarning(
                    entityType: typeof(TEntity),
                    entityId: id,
                    warning: "NotFound");
                return false;
            }

            await CloudTable.ExecuteAsync(
                TableOperation.Delete((ITableEntity)result.Result),
                cancellationToken)
                .ConfigureAwait(false);

            Logger.LogDeleted(
                entityType: typeof(TEntity),
                entityId: id);

            return true;
        }

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = Query.DefaultLimit, CancellationToken cancellationToken = default)
        {
            if (limit <= 0)
            {
                return Task.FromResult(ContinuationEnumerable.Empty<TEntity>());
            }

            return PrepareQueryAndExecuteAsync(
                new FindAllQuery<TEntity> { ContinuationToken = continuationToken, Limit = limit },
                _ => { },
                cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAsync(ICriteriaQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            string GenerateFilterCondtion(string targetPropertyPath, string operation, object value) => value switch
            {
                byte[] binary => TableQuery.GenerateFilterConditionForBinary(targetPropertyPath, operation, binary),
                bool boolean => TableQuery.GenerateFilterConditionForBool(targetPropertyPath, operation, boolean),
                DateTimeOffset date => TableQuery.GenerateFilterConditionForDate(targetPropertyPath, operation, date),
                double dble => TableQuery.GenerateFilterConditionForDouble(targetPropertyPath, operation, dble),
                Guid guid => TableQuery.GenerateFilterConditionForGuid(targetPropertyPath, operation, guid),
                int integer => TableQuery.GenerateFilterConditionForInt(targetPropertyPath, operation, integer),
                long lng => TableQuery.GenerateFilterConditionForLong(targetPropertyPath, operation, lng),
                _ => TableQuery.GenerateFilterCondition(targetPropertyPath, operation, value?.ToString()),
            };

            TableQuery<DynamicTableEntity> AddFilterCondition(TableQuery<DynamicTableEntity> tblQuery, PropertyValueCriterion criterion, string operation)
            {
                var targetPropertyPaths = Mapper.ResolveQueryPropertyPaths(criterion.PropertyPath);
                foreach (var targetPropertyPath in targetPropertyPaths)
                {
                    Logger.LogTrace("AddFilterCondition() {TargetPropertyPath} {Operation} {Value}", targetPropertyPath, operation, criterion.Value);
                    tblQuery = tblQuery.Where(GenerateFilterCondtion(targetPropertyPath, operation, criterion.Value!));
                }

                return tblQuery;
            }

            TableQuery<DynamicTableEntity> AddInCriterion(TableQuery<DynamicTableEntity> tblQuery, InPropertyCriterion criterion)
            {
                if (criterion.Values is null || !criterion.Values.Any())
                {
                    return tblQuery;
                }

                var targetPropertyPaths = Mapper.ResolveQueryPropertyPaths(criterion.PropertyPath);
                foreach (var targetPropertyPath in targetPropertyPaths)
                {
                    var currentFilter = string.Empty;
                    foreach (var value in criterion.Values)
                    {
                        var additionalFilter = GenerateFilterCondtion(targetPropertyPath, QueryComparisons.Equal, value);
                        if (string.IsNullOrEmpty(currentFilter))
                        {
                            currentFilter = additionalFilter;
                        }
                        else
                        {
                            currentFilter = TableQuery.CombineFilters(currentFilter, TableOperators.Or, additionalFilter);
                        }
                    }

                    tblQuery = tblQuery.Where(currentFilter);
                }

                return tblQuery;
            }

            TableQuery<DynamicTableEntity> AddOrderBy(TableQuery<DynamicTableEntity> tblQuery, OrderCriterion order)
            {
                var targetPropertyPaths = Mapper.ResolveQueryPropertyPaths(order.PropertyPath);
                if (targetPropertyPaths.Any())
                {
                    return order.Ascending
                        ? tblQuery.OrderBy(targetPropertyPaths.First())
                        : tblQuery.OrderByDesc(targetPropertyPaths.First());
                }

                return tblQuery;
            }

            return PrepareQueryAndExecuteAsync(
                query,
                tableQuery =>
                {
                    foreach (var criterion in query)
                    {
                        if (criterion is PropertyCriterion propertyCriterion && propertyCriterion.PropertyPathContainsSubPath)
                        {
                            throw new UnsupportedFeatureException($"{GetType().Name} does not support sub-property selection ({propertyCriterion.PropertyPath}).");
                        }

                        tableQuery = criterion switch
                        {
                            EqualToPropertyCriterion equalTo => AddFilterCondition(tableQuery, equalTo, QueryComparisons.Equal),
                            GreaterThanPropertyCriterion gt => AddFilterCondition(tableQuery, gt, QueryComparisons.GreaterThan),
                            GreaterThanOrEqualToPropertyCriterion gte => AddFilterCondition(tableQuery, gte, QueryComparisons.GreaterThanOrEqual),
                            LessThanPropertyCriterion lt => AddFilterCondition(tableQuery, lt, QueryComparisons.LessThan),
                            LessThanOrEqualToPropertyCriterion lte => AddFilterCondition(tableQuery, lte, QueryComparisons.LessThanOrEqual),
                            InPropertyCriterion inProp => AddInCriterion(tableQuery, inProp),
                            OrderCriterion order => AddOrderBy(tableQuery, order),
                            _ => throw new UnsupportedFeatureException($"The {criterion} criterion is not supported by {this}."),
                        };
                    }
                },
                cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAsync(AzureTableStorageTableQuery<TEntity> query, CancellationToken cancellationToken = default)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return PrepareQueryAndExecuteAsync(
                query,
                tableQuery => query.Apply(tableQuery),
                cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>
            {
                { nameof(CloudTable.StorageUri), CloudTable.StorageUri.ToString() },
                { "TableName", CloudTable.Name },
            };

            try
            {
                var exists = await CloudTable.ExistsAsync(cancellationToken).ConfigureAwait(false);
                if (exists)
                {
                    return HealthCheckResult.Healthy(data: data);
                }
                else
                {
                    return HealthCheckResult.Degraded($"The table {CloudTable.Name} does not appear to exists.", data: data);
                }
            }
            catch (StorageException ex)
            {
                Logger.LogError(ex, "An exception occured during the heatlh check: {Message}", ex.Message);
                return HealthCheckResult.Unhealthy(
                    description: $"A {nameof(StorageException)} occured during the check.",
                    exception: ex,
                    data: data);
            }
        }

        /// <summary>
        /// Parses Etags to extract timestamp.
        /// See https://github.com/Azure/azure-storage-net/blob/68c3ee55a3a6f62a0159cea58005d3fe027312a8/Lib/WindowsRuntime/Table/Protocol/TableOperationHttpResponseParsers.cs#L754
        /// and https://stackoverflow.com/questions/5500139/updating-an-object-to-azure-table-storage-is-there-any-way-to-get-the-new-time.
        /// </summary>
        /// <param name="etag">The ETag.</param>
        /// <returns>Parsed Timestamp.</returns>
        protected DateTimeOffset ParseETagForTimestamp(string etag)
        {
            if (etag is null)
            {
                throw new ArgumentNullException(nameof(etag));
            }

            const string ETagPrefix = "\"datetime'";
            // Handle strong ETags as well.
            if (etag.StartsWith("W/", StringComparison.Ordinal))
            {
                etag = etag.Substring(2);
            }

            // DateTimeOffset.ParseExact can't be used because the decimal part after seconds may not be present for rounded times.
            etag = etag.Substring(ETagPrefix.Length, etag.Length - 2 - ETagPrefix.Length);
            return DateTimeOffset.Parse(Uri.UnescapeDataString(etag), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }

        /// <summary>
        /// Prepares a <see cref="TableQuery{DynamicTableEntity}"/> for execution,
        /// allows its customization trough <paramref name="apply"/>, and then
        /// executes it and map out the result.
        /// </summary>
        /// <param name="query">The original query.</param>
        /// <param name="apply">Applies customization to the table query.</param>
        /// <param name="cancellationToken">the <see cref="CancellationToken"/>.</param>
        /// <returns>The mapped out results.</returns>
        protected virtual async Task<IContinuationEnumerable<TEntity>> PrepareQueryAndExecuteAsync(
            IQuery<TEntity> query,
            Action<TableQuery<DynamicTableEntity>> apply,
            CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (apply is null)
            {
                throw new ArgumentNullException(nameof(apply));
            }

            var tableQuery = new TableQuery<DynamicTableEntity>();
            tableQuery.Take(query.Limit);
            apply(tableQuery);

            var operationResult = await CloudTable.ExecuteQuerySegmentedAsync(
                tableQuery,
                await ContinuationTokenEncoder.Decode(query.ContinuationToken),
                cancellationToken).ConfigureAwait(false);

            var result = operationResult.Results
                .Select(x => Mapper.MapToSource(x))
                .ToContinuationEnumerable(await ContinuationTokenEncoder.Encode(operationResult.ContinuationToken));

            Logger.LogFind(query, result);

            return result;
        }
    }
}
