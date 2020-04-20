using Pantry.PetaPoco.Queries;
using Pantry.Tests.StandardTestSupport;
using SqlKata;

namespace Pantry.PetaPoco.Tests
{
    public class CustomPetaPocoSqlBuilderQuery : PetaPocoSqlBuilderQuery<StandardEntity>
    {
        public string NameEq { get; set; } = string.Empty;

        public override void Apply(Query sqlQuery)
        {
            if (sqlQuery is null)
            {
                throw new System.ArgumentNullException(nameof(sqlQuery));
            }

            if (!string.IsNullOrEmpty(NameEq))
            {
                sqlQuery.Where("Name", NameEq);
            }
        }
    }
}
