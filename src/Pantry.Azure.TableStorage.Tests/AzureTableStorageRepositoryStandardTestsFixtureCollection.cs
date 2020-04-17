using Xunit;

namespace Pantry.Azure.TableStorage.Tests
{
    [CollectionDefinition(CollectionName)]
    public class AzureTableStorageRepositoryStandardTestsFixtureCollection : ICollectionFixture<AzureTableStorageRepositoryStandardTestsFixture>
    {
        public const string CollectionName = nameof(AzureTableStorageRepositoryStandardTestsFixtureCollection);
    }
}
