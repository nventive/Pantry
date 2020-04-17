using System;
using System.Collections.Generic;
using System.Linq;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Pantry.Tests.StandardTestSupport
{
    public abstract class StandardRepositoryImplementationTestsFixture : ITestOutputHelperAccessor
    {
        private readonly Lazy<IHost> _lazyHost;

        protected StandardRepositoryImplementationTestsFixture()
        {
            _lazyHost = new Lazy<IHost>(BuildHost);
        }

        /// <summary>
        /// Gets or sets the <see cref="ITestOutputHelper"/>.
        /// </summary>
        public ITestOutputHelper? OutputHelper { get; set; }

        /// <summary>
        /// Gets the <see cref="IHost"/>.
        /// </summary>
        public IHost Host => _lazyHost.Value;

        protected IHost BuildHost()
        {
            var hostBuilder = Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config
                        .AddInMemoryCollection(AdditionalConfigurationValues())
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging(logging =>
                {
                    logging
                        .AddXUnit(OutputHelper)
                        .AddFilter(_ => true);
                })
                .ConfigureServices((context, services) =>
                {
                    RegisterTestServices(context, services);
                });

            PostConfigureHostBuilder(hostBuilder);

            return hostBuilder.Build();
        }

        /// <summary>
        /// Registers the services under test.
        /// </summary>
        /// <param name="context">The <see cref="HostBuilderContext"/>.</param>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        protected abstract void RegisterTestServices(HostBuilderContext context, IServiceCollection services);

        /// <summary>
        /// Additional values to add to the <see cref="IConfiguration"/>.
        /// Those can be overriden by environment variables after.
        /// </summary>
        /// <returns>The list of configuration variables.</returns>
        protected virtual IEnumerable<KeyValuePair<string, string>> AdditionalConfigurationValues() => Enumerable.Empty<KeyValuePair<string, string>>();

        /// <summary>
        /// Any additional configuration to apply to the <see cref="IHostBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHostBuilder"/>.</param>
        protected virtual void PostConfigureHostBuilder(IHostBuilder builder)
        {
            // No-op.
        }
    }
}
