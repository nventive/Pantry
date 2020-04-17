using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Pantry.Azure.Cosmos.Queries;
using Pantry.Tests.StandardTestSupport;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Azure.Cosmos.Tests
{
    [Collection(CosmosStandardTestsFixtureCollection.CollectionName)]
    public class CosmosStandardTests : StandardRepositoryImplementationTests<CosmosRepository<StandardEntity>>
    {
        public CosmosStandardTests(CosmosStandardTestsFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldExecuteCustomSqlQueries()
        {
            var repo = GetRepositoryAs<IRepositoryFind<StandardEntity, StandardEntity, CosmosSqlQuery<StandardEntity>>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<StandardEntity>(repo, entities);

            var targetEntity = Faker.PickRandom(entities);
            var query = new CustomCosmosSqlQuery { RelatedNameEq = targetEntity.Related!.Name! };
            var result = await repo.FindAsync(query);

            result.Should().HaveCountGreaterOrEqualTo(1);
            result.Select(x => x.Id).Should().Contain(targetEntity.Id);
        }

        [Fact]
        public async Task ItShouldExecuteCustomSqlBuilderQueries()
        {
            var repo = GetRepositoryAs<IRepositoryFind<StandardEntity, StandardEntity, CosmosSqlBuilderQuery<StandardEntity>>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<StandardEntity>(repo, entities);

            var targetEntity = Faker.PickRandom(entities);
            var query = new CustomCosmosSqlBuilderQuery { NameEq = targetEntity.Name! };
            var result = await repo.FindAsync(query);

            result.Should().HaveCountGreaterOrEqualTo(1);
            result.Select(x => x.Id).Should().Contain(targetEntity.Id);
        }
    }
}
