using Xunit;

namespace Pantry.PetaPoco.Tests
{
    [CollectionDefinition(CollectionName)]
    public class PetaPocoStandardTestsFixtureCollection : ICollectionFixture<PetaPocoStandardTestsFixture>
    {
        public const string CollectionName = nameof(PetaPocoStandardTestsFixtureCollection);
    }
}
