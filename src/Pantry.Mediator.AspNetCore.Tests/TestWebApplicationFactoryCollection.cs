using Xunit;

namespace Pantry.Mediator.AspNetCore.Tests
{
    [CollectionDefinition(CollectionName)]
    public class TestWebApplicationFactoryCollection : ICollectionFixture<TestWebApplicationFactory>
    {
        public const string CollectionName = nameof(TestWebApplicationFactoryCollection);
    }
}
