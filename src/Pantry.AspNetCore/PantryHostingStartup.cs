using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Pantry.AspNetCore.ApplicationModels;
using Pantry.AspNetCore.Filters;

[assembly: HostingStartup(typeof(Pantry.AspNetCore.PantryHostingStartup))]

namespace Pantry.AspNetCore
{
    /// <summary>
    /// Automatically configures the Pantry options.
    /// </summary>
    public class PantryHostingStartup : IHostingStartup
    {
        /// <inheritdoc/>
        public void Configure(IWebHostBuilder builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.ConfigureServices(services =>
            {
                services.Configure<MvcOptions>(options =>
                {
                    options.Conventions.Add(new CapabilitiesApplicationModelConvention());
                    options.Filters.Add<EntityHttpCacheResponseHeadersAttribute>();
                });
            });
        }
    }
}
