using Xunit;

namespace Pantry.Redis.Tests
{
    [CollectionDefinition(CollectionName)]
    public class RedisStandardTestsFixtureCollection : ICollectionFixture<RedisStandardTestsFixture>
    {
        public const string CollectionName = nameof(RedisStandardTestsFixtureCollection);
    }
}
