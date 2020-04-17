using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Pantry.InMemory.Queries;
using Pantry.Tests.StandardTestSupport;
using Pantry.Traits;
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

        [Fact]
        public async Task ItShouldExecuteCustomLinqQueries()
        {
            var repo = GetRepositoryAs<IRepositoryFind<StandardEntity, StandardEntity, InMemoryLinqQuery<StandardEntity>>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<StandardEntity>(repo, entities);

            var targetEntity = Faker.PickRandom(entities);
            var query = new CustomLinqQuery { RelatedNameEq = targetEntity.Related!.Name! };
            var result = await repo.FindAsync(query);

            result.Should().HaveCountGreaterOrEqualTo(1);
            result.Select(x => x.Id).Should().Contain(targetEntity.Id);
        }
    }
}
