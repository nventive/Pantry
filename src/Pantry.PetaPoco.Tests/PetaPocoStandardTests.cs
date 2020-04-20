using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Pantry.PetaPoco.Queries;
using Pantry.Tests.StandardTestSupport;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.PetaPoco.Tests
{
    [Collection(PetaPocoStandardTestsFixtureCollection.CollectionName)]
    public class PetaPocoStandardTests : StandardRepositoryImplementationTests<PetaPocoRepository<StandardEntity>>
    {
        public PetaPocoStandardTests(
            PetaPocoStandardTestsFixture fixture,
            ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldExecuteCustomSqlBuilderQueries()
        {
            var repo = GetRepositoryAs<IRepositoryFind<StandardEntity, StandardEntity, PetaPocoSqlBuilderQuery<StandardEntity>>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<StandardEntity>(repo, entities);

            var targetEntity = Faker.PickRandom(entities);
            var query = new CustomPetaPocoSqlBuilderQuery { NameEq = targetEntity.Name! };
            var result = await repo.FindAsync(query);

            result.Should().HaveCountGreaterOrEqualTo(1);
            result.Select(x => x.Id).Should().Contain(targetEntity.Id);
        }
    }
}
