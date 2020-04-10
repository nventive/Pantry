using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;
using Xunit.Abstractions;

namespace Pantry.InMemory.Tests
{
    public class ConcurrentDictionaryStandardTests : StandardRepositoryImplementationTests
    {
        public ConcurrentDictionaryStandardTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override void RegisterTestServices<TEntity>(HostBuilderContext context, IServiceCollection services)
        {
            services.AddConcurrentDictionaryRepository<TEntity>();
        }
    }
}
