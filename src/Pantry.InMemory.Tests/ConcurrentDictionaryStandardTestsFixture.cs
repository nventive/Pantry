using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.InMemory.Tests
{
    public class ConcurrentDictionaryStandardTestsFixture : StandardRepositoryImplementationTestsFixture
    {
        protected override void RegisterTestServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddConcurrentDictionaryRepository<StandardEntity>();
        }
    }
}
