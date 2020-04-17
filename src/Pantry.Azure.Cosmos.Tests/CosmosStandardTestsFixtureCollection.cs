using Xunit;

namespace Pantry.Azure.Cosmos.Tests
{
    [CollectionDefinition(CollectionName)]
    public class CosmosStandardTestsFixtureCollection : ICollectionFixture<CosmosStandardTestsFixture>
    {
        public const string CollectionName = nameof(CosmosStandardTestsFixtureCollection);
    }
}
