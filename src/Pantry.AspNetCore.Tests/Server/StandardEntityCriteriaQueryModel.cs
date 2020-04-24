using System.Collections.Generic;

namespace Pantry.AspNetCore.Tests.Server
{
    public class StandardEntityCriteriaQueryModel
    {
        public int? Limit { get; set; }

        public string? ContinuationToken { get; set; }

        public string? IdEq { get; set; }

        public string? NameEq { get; set; }

        public string? NameLike { get; set; }

        public int? AgeEq { get; set; }

        public int? AgeGt { get; set; }

        public int? AgeGte { get; set; }

        public int? AgeLt { get; set; }

        public int? AgeLte { get; set; }

        public IEnumerable<int>? AgeIn { get; set; }

        public string? OrderBy { get; set; }
    }
}
