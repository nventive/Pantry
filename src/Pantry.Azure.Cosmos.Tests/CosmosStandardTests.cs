using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;
using Xunit.Abstractions;

namespace Pantry.Azure.Cosmos.Tests
{
    public class CosmosStandardTests : StandardRepositoryImplementationTests<CosmosRepository<StandardEntity>>
    {
        public CosmosStandardTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override void RegisterTestServices<TEntity>(HostBuilderContext context, IServiceCollection services)
        {
            services.AddCosmosRepository<TEntity>();
        }
    }
}
