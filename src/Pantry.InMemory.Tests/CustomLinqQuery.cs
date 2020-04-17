using System.Linq;
using Pantry.InMemory.Queries;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.InMemory.Tests
{
    public class CustomLinqQuery : InMemoryLinqQuery<StandardEntity>
    {
        public string RelatedNameEq { get; set; } = string.Empty;

        public override IQueryable<StandardEntity> Apply(IQueryable<StandardEntity> queryable)
            => queryable.Where(x => x.Related!.Name == RelatedNameEq);
    }
}
