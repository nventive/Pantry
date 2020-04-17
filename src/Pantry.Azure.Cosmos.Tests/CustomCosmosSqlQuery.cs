using Microsoft.Azure.Cosmos;
using Pantry.Azure.Cosmos.Queries;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.Azure.Cosmos.Tests
{
    public class CustomCosmosSqlQuery : CosmosSqlQuery<StandardEntity>
    {
        public string RelatedNameEq { get; set; } = string.Empty;

        public override QueryDefinition GetQueryDefinition()
            => new QueryDefinition($"SELECT * FROM e WHERE e._type = 'StandardEntity' AND e.Related.Name = '{RelatedNameEq}'");
    }
}
