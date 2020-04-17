using Pantry.Azure.Cosmos.Queries;
using Pantry.Tests.StandardTestSupport;
using SqlKata;

namespace Pantry.Azure.Cosmos.Tests
{
    public class CustomCosmosSqlBuilderQuery : CosmosSqlBuilderQuery<StandardEntity>
    {
        public string NameEq { get; set; } = string.Empty;

        public override Query GetQuery()
            => new SqlKata.Query("e")
                .Select("*")
                .Where($"e.{CosmosDocument.TypeAttribute}", "StandardEntity")
                .Where("e.Name", NameEq);
    }
}
