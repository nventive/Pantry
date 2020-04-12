using System;
using HttpTracing;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pantry.AspNetCore.Tests.Server;
using Refit;
using Xunit.Abstractions;

namespace Pantry.AspNetCore.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>, ITestOutputHelperAccessor
    {
        public ITestOutputHelper? OutputHelper { get; set; }

        public T GetApiClient<T>()
        {
            return RestService.For<T>(
                CreateDefaultClient(new[]
                {
                    new HttpTracingDelegatingHandler(Services.GetRequiredService<ILogger<T>>(), bufferRequests: true),
                }),
                new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(
                        Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions),
                });
        }

        protected override IHostBuilder CreateHostBuilder()
            => Host
                .CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.AddXUnit(OutputHelper);
                    logging.AddFilter((category, level) => category.EndsWith("TraceHandler", StringComparison.OrdinalIgnoreCase));
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
