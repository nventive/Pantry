using System;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pantry;
using Pantry.Azure.TableStorage;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IHealthChecksBuilder"/> extension methods.
    /// </summary>
    public static class AzureTableStorageHealthChecksBuilderExtensions
    {
        /// <summary>
        /// Adds a health check for <see cref="AzureTableStorageRepository{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The health check name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check reports a failure.
        /// If the provided value is null, then <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">
        /// A list of tags that can be used for filtering health checks.
        /// </param>
        /// <returns>The updated <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureTableStorageRepositoryCheck<TEntity>(
            this IHealthChecksBuilder builder,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default)
            where TEntity : class, IIdentifiable, new()
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Add(
                new HealthCheckRegistration(
                    name ?? typeof(AzureTableStorageRepository<TEntity>).FullName,
                    sp => sp.GetRequiredService<AzureTableStorageRepository<TEntity>>(),
                    failureStatus,
                    tags));
        }
    }
}
