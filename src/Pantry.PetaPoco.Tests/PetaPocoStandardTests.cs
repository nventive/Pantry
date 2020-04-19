using Pantry.Tests.StandardTestSupport;
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
    }
}
