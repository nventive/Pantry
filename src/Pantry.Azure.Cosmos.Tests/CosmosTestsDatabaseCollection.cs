using Xunit;

namespace Pantry.Azure.Cosmos.Tests
{
    [CollectionDefinition(CollectionName)]
    public class CosmosTestsDatabaseCollection : ICollectionFixture<CosmosTestsDatabase>
    {
        public const string CollectionName = nameof(CosmosTestsDatabaseCollection);
    }
}
