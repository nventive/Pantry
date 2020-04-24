using System;

namespace Pantry.AspNetCore.Tests.Server
{
    public class StandardEntityModel : SummaryStandardEntityModel, IETaggable, ITimestamped
    {
        public string? ETag { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public int Age { get; set; }
    }
}
