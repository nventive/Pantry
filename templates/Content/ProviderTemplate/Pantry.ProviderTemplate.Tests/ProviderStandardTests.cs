using Pantry.Tests.StandardTestSupport;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.ProviderTemplate.Tests
{
    [Collection(ProviderStandardTestsFixtureCollection.CollectionName)]
    public class ProviderStandardTests : StandardRepositoryImplementationTests<ProviderRepository<StandardEntity>>
    {
        public ProviderStandardTests(
            ProviderStandardTestsFixture fixture,
            ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }
    }
}
