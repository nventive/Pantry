using System;
using System.Linq;
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
        }

        /// <inheritdoc/>
        public Task<IContinuationEnumerable<TResult>> ExecuteAsync<TResult, TQuery>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResult>
        {
            var handler = FindTargetHandler<TResult, TQuery>(query);

            var method = handler.GetType().GetMethod("ExecuteAsync");
            if (method is null)
            {
                throw new InternalErrorException($"Unable to find method named ExecuteAsync on {handler.GetType()}.");
            }

            return (Task<IContinuationEnumerable<TResult>>)method.Invoke(handler, new object[] { query, cancellationToken });
        }

        private object FindTargetHandler<TResult, TQuery>(TQuery query)
            where TQuery : IQuery<TResult>
        {
            var targetQueryHandlerType = typeof(IQueryHandler<,,>)
                .MakeGenericType(typeof(TEntity), typeof(TResult), query.GetType());

            // TODO: avoid unecessary instantiations.
            var handler = _serviceProvider
                .GetServices<TQueryHandlerBase>()
                .FirstOrDefault(x => targetQueryHandlerType.IsInstanceOfType(x));

            if (handler is null)
            {
                throw new UnsupportedFeatureException($"Query {query} has no registered handler with a base type of {typeof(TQueryHandlerBase)}.");
            }

            return handler;
        }
    }
}
