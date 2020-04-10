using System.Linq;
using Bogus;
using Xunit.Abstractions;

namespace Pantry.Tests.StandardTestSupport
{
    /// <summary>
    /// Base class for standard test suit that repository implementation can use & follow.
    /// </summary>
    public abstract class StandardRepositoryImplementationTests : StandardRepositoryImplementationTests<StandardEntity, AllQueryStandardEntity>
    {
        public StandardRepositoryImplementationTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override Faker<StandardEntity> TestEntityGenerator => new Faker<StandardEntity>()
            .RuleFor(x => x.Name, f => f.Person.UserName)
            .RuleFor(x => x.Age, f => f.Random.Int(1, 100))
            .RuleFor(x => x.Related, f => SubTestEntityGenerator.Generate())
            .RuleFor(x => x.Lines, f => SubTestEntityGenerator.Generate(3).ToList());

        protected virtual Faker<SubStandardEntity> SubTestEntityGenerator => new Faker<SubStandardEntity>()
            .RuleFor(x => x.Name, f => f.Lorem.Word());
    }
}
