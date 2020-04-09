using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;

namespace Pantry.Queries
{
    /// <summary>
    /// <see cref="IQueryHandlerExecutor{TEntity, TQueryHandlerBase}"/> implementation that uses <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type. </typeparam>
    /// <typeparam name="TQueryHandlerBase">The query handler base type used to identify the factory.</typeparam>
    public class ServiceProviderQueryHandlerExecutor<TEntity, TQueryHandlerBase> : IQueryHandlerExecutor<TEntity, TQueryHandlerBase>
        where TEntity : class, IIdentifiable
    {
        private readonly Lazy<IDictionary<Type, (Type, MethodInfo)>> _cachedQueryhandlerResolution;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderQueryHandlerExecutor{TEntity, TQueryHandlerBase}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ServiceProviderQueryHandlerExecutor(
            IServiceProvider serviceProvider,
            ILogger<ServiceProviderQueryHandlerExecutor<TEntity, TQueryHandlerBase>>? logger = null)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? NullLogger<ServiceProviderQueryHandlerExecutor<TEntity, TQueryHandlerBase>>.Instance;
            _cachedQueryhandlerResolution = new Lazy<IDictionary<Type, (Type, MethodInfo)>>(LoadQueryHandlerResolution, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <inheritdoc/>
        public Task<IContinuationEnumerable<TResult>> ExecuteAsync<TResult, TQuery>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResult>
        {
            (Type, MethodInfo) handlerInfo;
            var currentQueryTypeLook = query.GetType();
            while (
                !_cachedQueryhandlerResolution.Value.TryGetValue(currentQueryTypeLook, out handlerInfo)
                && currentQueryTypeLook != typeof(object))
            {
                currentQueryTypeLook = currentQueryTypeLook.BaseType;
            }

            if (handlerInfo == default)
            {
                var exception = new UnsupportedFeatureException($"Query {query} has no registered handler with a base type of {typeof(TQueryHandlerBase)}.");
                _logger.LogWarning(exception, "QueryHandlerNotFound: {Query} {QueryHandlerBaseType}", query, typeof(TQueryHandlerBase));
                throw exception;
            }

            var handler = _serviceProvider.GetService(handlerInfo.Item1);
            if (handler is null)
            {
                var exception = new InternalErrorException($"Unable to find handler {handlerInfo.Item1} as a service; did you forget to register it under its own implementation type as well?");
                _logger.LogWarning(exception, "QueryHandlerNotRegisteredAsSelf: {QueryHandlerType}", handlerInfo.Item1);
                throw exception;
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("ExecutingQueryHandler: {Query} {QueryHandler}", query, handler);
            }

            return (Task<IContinuationEnumerable<TResult>>)handlerInfo.Item2.Invoke(handler, new object[] { query, cancellationToken });
        }

        private IDictionary<Type, (Type, MethodInfo)> LoadQueryHandlerResolution()
        {
            var result = new Dictionary<Type, (Type, MethodInfo)>();

            foreach (var handler in _serviceProvider.GetServices<TQueryHandlerBase>())
            {
                var handlerType = handler?.GetType();
                if (handlerType is null)
                {
                    continue;
                }

                var queryHandlerInterface = handlerType.GetInterfaces()
                    .FirstOrDefault(x => x.GetGenericTypeDefinition() == typeof(IQueryHandler<,,>));
                if (queryHandlerInterface is null)
                {
                    continue;
                }

                var methodInfo = handlerType.GetMethod("ExecuteAsync");
                if (methodInfo is null)
                {
                    continue;
                }

                var queryType = queryHandlerInterface.GetGenericArguments()[2];
                result[queryType] = (handlerType, methodInfo);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "QueryHandlerMapping: {QueryHandlerMapping}",
                    string.Join(Environment.NewLine, result.Select(x => $"{x.Key.FullName}={x.Value.Item1.FullName}")));
            }

            return result;
        }
    }
}
