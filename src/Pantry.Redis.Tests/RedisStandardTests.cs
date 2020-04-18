using Pantry.Tests.StandardTestSupport;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Redis.Tests
{
    [Collection(RedisStandardTestsFixtureCollection.CollectionName)]
    public class RedisStandardTests : StandardRepositoryImplementationTests<RedisRepository<StandardEntity>>
    {
        public RedisStandardTests(
            RedisStandardTestsFixture fixture,
            ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }
    }
}
