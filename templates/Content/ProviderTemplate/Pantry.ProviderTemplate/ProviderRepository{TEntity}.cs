using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Queries;

namespace Pantry.ProviderTemplate
{
    /// <summary>
    /// Provider Repository Implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class ProviderRepository<TEntity> : Repository<TEntity>, IHealthCheck
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public ProviderRepository(
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <inheritdoc/>
        public virtual async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>
            {
            };

            // Check health
            return HealthCheckResult.Healthy(data: data);
        }
    }
}
