using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Pantry.Exceptions;
using Pantry.Queries;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Tests.StandardTestSupport
{
    /// <summary>
    /// Base class for standard test suit that repository implementation can use & follow.
    /// </summary>
    /// <typeparam name="TRepository">The type of repository to test.</typeparam>
    public abstract class StandardRepositoryImplementationTests<TRepository> : StandardRepositoryImplementationTests<TRepository, StandardEntity>
    {
        public StandardRepositoryImplementationTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override Faker<StandardEntity> TestEntityGenerator => new Faker<StandardEntity>()
            .RuleFor(x => x.Name, f => f.Person.UserName)
            .RuleFor(x => x.Age, f => f.Random.Int(1, 100))
            .RuleFor(x => x.NotarizedAt, f => f.Date.PastOffset())
            .RuleFor(x => x.Related, f => SubTestEntityGenerator.Generate())
            .RuleFor(x => x.Lines, f => SubTestEntityGenerator.Generate(3).ToList());

        protected virtual Faker<SubStandardEntity> SubTestEntityGenerator => new Faker<SubStandardEntity>()
            .RuleFor(x => x.Name, f => f.Lorem.Word());

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldFindWithASimpleCriteriaQuery()
        {
            var repo = GetRepositoryAs<IRepositoryFindByCriteria<StandardEntity>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<StandardEntity>(repo, entities);

            var query = new CriteriaQuery<StandardEntity>()
                .Eq(x => x.Name, entities.Last().Name);
            var result = await repo.FindAsync(query);

            result.Should().NotBeEmpty();
            result.Select(x => x.Id).Should().Contain(entities.Last().Id);
        }
    }
}
