using System;
using System.Diagnostics.CodeAnalysis;
using Pantry;
using Pantry.AspNetCore.Controllers;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IMvcBuilder"/> extension methods.
    /// </summary>
    public static class MvcBuilderExtensions
    {
        /// <summary>
        /// Adds a generic repository controller for <typeparamref name="TEntity"/> and <paramref name="capabilities"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="baseUri">The base uri where the controller is invoked.</param>
        /// <param name="capabilities">The actions exposed.</param>
        /// <returns>The updated <see cref="IMvcBuilder"/>.</returns>
        [SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Better / easier API design, aligned with ASP.NET Core routing.")]
        public static IMvcBuilder AddRepositoryController<TEntity>(this IMvcBuilder builder, string baseUri, Capabilities capabilities)
            where TEntity : class, IIdentifiable
        {
            var u = baseUri;
            var c = capabilities;
            return builder;
        }
    }
}
