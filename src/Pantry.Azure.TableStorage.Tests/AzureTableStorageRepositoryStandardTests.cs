using Pantry.Tests.StandardTestSupport;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Azure.TableStorage.Tests
{
    [Collection(AzureTableStorageRepositoryStandardTestsFixtureCollection.CollectionName)]
    public class AzureTableStorageRepositoryStandardTests : StandardRepositoryImplementationTests<AzureTableStorageRepository<StandardEntity>>
    {
        public AzureTableStorageRepositoryStandardTests(
            AzureTableStorageRepositoryStandardTestsFixture fixture,
            ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }
    }
}
