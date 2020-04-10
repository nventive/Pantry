using System;
using System.Collections.Generic;

namespace Pantry.Tests.StandardTestSupport
{
    public class StandardEntity : RootAggregateEntity
    {
        public string? Name { get; set; }

        public int Age { get; set; }

        public DateTimeOffset? NotarizedAt { get; set; }

        public SubStandardEntity? Related { get; set; }

        public List<SubStandardEntity> Lines { get; set; } = new List<SubStandardEntity>();
    }
}
