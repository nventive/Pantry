using Pantry.AspNetCore.ApplicationModels;
using Pantry.AspNetCore.Filters;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IMvcBuilder"/> extension methods.
    /// </summary>
    public static class MvcBuilderExtensions
    {
        /// <summary>
        /// Configure MVC to support Pantry Generic controllers and HTTP cache.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <returns>The updated <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddPantry(this IMvcBuilder builder)
        {
            builder.AddMvcOptions(options =>
            {
                options.Conventions.Add(new CapabilitiesApplicationModelConvention());
                options.Filters.Add<EntityHttpCacheResponseHeadersAttribute>();
            });

            return builder;
        }
    }
}
