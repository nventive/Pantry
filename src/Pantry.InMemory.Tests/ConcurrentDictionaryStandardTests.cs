using Pantry.Tests.StandardTestSupport;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.InMemory.Tests
{
    [Collection(ConcurrentDictionaryStandardTestsFixtureCollection.CollectionName)]
    public class ConcurrentDictionaryStandardTests : StandardRepositoryImplementationTests<ConcurrentDictionaryRepository<StandardEntity>>
    {
        public ConcurrentDictionaryStandardTests(
            ConcurrentDictionaryStandardTestsFixture fixture,
            ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }
    }
}
