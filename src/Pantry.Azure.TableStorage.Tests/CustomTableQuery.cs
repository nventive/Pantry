using System;
using Microsoft.Azure.Cosmos.Table;
using Pantry.Azure.TableStorage.Queries;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.Azure.TableStorage.Tests
{
    public class CustomTableQuery : AzureTableStorageTableQuery<StandardEntity>
    {
        public string NameEq { get; set; } = string.Empty;

        public override void Apply(TableQuery<DynamicTableEntity> tableQuery)
        {
            if (tableQuery is null)
            {
                throw new ArgumentNullException(nameof(tableQuery));
            }

            tableQuery.Where(TableQuery.GenerateFilterCondition(nameof(StandardEntity.Name), QueryComparisons.Equal, NameEq));
        }
    }
}
