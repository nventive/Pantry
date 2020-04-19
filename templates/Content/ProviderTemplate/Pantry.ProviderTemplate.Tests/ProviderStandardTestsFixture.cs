using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.ProviderTemplate.Tests
{
    public class ProviderStandardTestsFixture : StandardRepositoryImplementationTestsFixture
    {
        protected override void RegisterTestServices(HostBuilderContext context, IServiceCollection services)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            services.AddProviderRepository<StandardEntity>();

            services
                .AddHealthChecks()
                .AddProviderRepositoryCheck<StandardEntity>();
        }
    }
}
