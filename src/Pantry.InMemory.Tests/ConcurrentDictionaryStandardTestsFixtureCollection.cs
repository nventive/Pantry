using Xunit;

namespace Pantry.InMemory.Tests
{
    [CollectionDefinition(CollectionName)]
    public class ConcurrentDictionaryStandardTestsFixtureCollection : ICollectionFixture<ConcurrentDictionaryStandardTestsFixture>
    {
        public const string CollectionName = nameof(ConcurrentDictionaryStandardTestsFixtureCollection);
    }
}
