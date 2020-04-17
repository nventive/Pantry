using Xunit;

namespace Pantry.ProviderTemplate.Tests
{
    [CollectionDefinition(CollectionName)]
    public class ProviderStandardTestsFixtureCollection : ICollectionFixture<ProviderStandardTestsFixture>
    {
        public const string CollectionName = nameof(ProviderStandardTestsFixtureCollection);
    }
}
