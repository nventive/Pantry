using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;
using Xunit.Abstractions;

namespace Pantry.ProviderTemplate.Tests
{
    public class ProviderStandardTests : StandardRepositoryImplementationTests
    {
        public ProviderStandardTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override void RegisterTestServices<TEntity>(HostBuilderContext context, IServiceCollection services)
        {
            services.AddProviderRepository<TEntity>();
        }
    }
}
