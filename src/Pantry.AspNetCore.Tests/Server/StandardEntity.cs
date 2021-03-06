﻿using System;

namespace Pantry.AspNetCore.Tests.Server
{
    public class StandardEntity : StandardEntityCreateModel, IIdentifiable, IETaggable, ITimestamped
    {
        public string Id { get; set; } = string.Empty;

        public string? ETag { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
    }
}
