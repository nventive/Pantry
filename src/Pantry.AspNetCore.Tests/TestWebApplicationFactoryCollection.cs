using Xunit;

namespace Pantry.AspNetCore.Tests
{
    [CollectionDefinition(CollectionName)]
    public class TestWebApplicationFactoryCollection : ICollectionFixture<TestWebApplicationFactory>
    {
        public const string CollectionName = nameof(TestWebApplicationFactoryCollection);
    }
}
