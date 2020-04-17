using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Pantry.Azure.TableStorage.Queries;
using Pantry.Tests.StandardTestSupport;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Azure.TableStorage.Tests
{
    [Collection(AzureTableStorageRepositoryStandardTestsFixtureCollection.CollectionName)]
    public class AzureTableStorageRepositoryStandardTests : StandardRepositoryImplementationTests<AzureTableStorageRepository<StandardEntity>>
    {
        public AzureTableStorageRepositoryStandardTests(
            AzureTableStorageRepositoryStandardTestsFixture fixture,
            ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldExecuteCustomSqlQueries()
        {
            var repo = GetRepositoryAs<IRepositoryFind<StandardEntity, StandardEntity, AzureTableStorageTableQuery<StandardEntity>>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<StandardEntity>(repo, entities);

            var targetEntity = Faker.PickRandom(entities);
            var query = new CustomTableQuery { NameEq = targetEntity.Name! };
            var result = await repo.FindAsync(query);

            result.Should().HaveCountGreaterOrEqualTo(1);
            result.Select(x => x.Id).Should().Contain(targetEntity.Id);
        }
    }
}
