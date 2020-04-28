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
using Pantry.Mediator.AspNetCore.Tests.Server;
using Refit;
using Xunit.Abstractions;

namespace Pantry.Mediator.AspNetCore.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Startup>, ITestOutputHelperAccessor
    {
        public ITestOutputHelper? OutputHelper { get; set; }

        public T GetApiClient<T>(string? relativeUri = null)
        {
            var client = CreateDefaultClient(new[]
                {
                    new HttpTracingDelegatingHandler(Services.GetRequiredService<ILogger<T>>(), bufferRequests: true),
                });

            if (!string.IsNullOrEmpty(relativeUri))
            {
                client.BaseAddress = new Uri(client.BaseAddress!, relativeUri);
            }

            return RestService.For<T>(
                client,
                new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(
                        Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions),
                    CollectionFormat = CollectionFormat.Multi,
                });
        }

        protected override IHostBuilder CreateHostBuilder()
            => Host
                .CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.AddXUnit(OutputHelper).AddFilter(_ => true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>();
                });
    }
}
