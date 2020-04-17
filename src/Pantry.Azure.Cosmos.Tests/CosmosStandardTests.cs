using Pantry.Tests.StandardTestSupport;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Azure.Cosmos.Tests
{
    [Collection(CosmosStandardTestsFixtureCollection.CollectionName)]
    public class CosmosStandardTests : StandardRepositoryImplementationTests<CosmosRepository<StandardEntity>>
    {
        public CosmosStandardTests(CosmosStandardTestsFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }
    }
}
