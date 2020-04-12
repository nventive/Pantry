using System;
using Pantry.Queries;
using Pantry.Queries.Criteria;

namespace Pantry.Tests.StandardTestSupport
{
    public class StandardEntityCriteriaQuery : CriteriaQuery<StandardEntity>
    {
        public string? NameEq
        {
            get => this.EqualToValue(x => x.Name);
            set => this.EqualTo(x => x.Name, value);
        }

        public string? NameLike
        {
            get => this.StringContainsValue(x => x.Name);
            set => this.StringContains(x => x.Name, value);
        }

        public int? AgeEq
        {
            get => this.EqualToValue(x => x.Age);
            set => this.EqualTo(x => x.Age, value);
        }

        public int? AgeGt
        {
            get => this.GreaterThanValue(x => x.Age);
            set => this.GreaterThan(x => x.Age, value);
        }

        public int? AgeGte
        {
            get => this.GreaterThanOrEqualToValue(x => x.Age);
            set => this.GreaterThanOrEqualTo(x => x.Age, value);
        }

        public int? AgeLt
        {
            get => this.LessThanValue(x => x.Age);
            set => this.LessThan(x => x.Age, value);
        }

        public int? AgeLte
        {
            get => this.LessThanOrEqualToValue(x => x.Age);
            set => this.LessThanOrEqualTo(x => x.Age, value);
        }

        public DateTimeOffset? NotarizedAtEq
        {
            get => this.EqualToValue(x => x.NotarizedAt);
            set => this.EqualTo(x => x.NotarizedAt, value);
        }

        public DateTimeOffset? NotarizedAtGt
        {
            get => this.GreaterThanValue(x => x.NotarizedAt);
            set => this.GreaterThan(x => x.NotarizedAt, value);
        }

        public DateTimeOffset? NotarizedAtGte
        {
            get => this.GreaterThanOrEqualToValue(x => x.NotarizedAt);
            set => this.GreaterThanOrEqualTo(x => x.NotarizedAt, value);
        }

        public DateTimeOffset? NotarizedAtLt
        {
            get => this.LessThanValue(x => x.NotarizedAt);
            set => this.LessThan(x => x.NotarizedAt, value);
        }

        public DateTimeOffset? NotarizedAtLte
        {
            get => this.LessThanOrEqualToValue(x => x.NotarizedAt);
            set => this.LessThanOrEqualTo(x => x.NotarizedAt, value);
        }

        public string? RelatedNameEq
        {
            get => this.GreaterThanValue(x => x.Related!.Name);
            set => this.GreaterThan(x => x.Related!.Name, value);
        }

        public string? RelatedNameLike
        {
            get => this.StringContainsValue(x => x.Related!.Name);
            set => this.StringContains(x => x.Related!.Name, value);
        }

        public string? LinesNameEq
        {
            get => this.GreaterThanValue(x => x.Lines[0].Name);
            set => this.GreaterThan(x => x.Lines[0].Name, value);
        }

        public string? LinesNameLike
        {
            get => this.StringContainsValue(x => x.Lines[0].Name);
            set => this.StringContains(x => x.Lines[0].Name, value);
        }
    }
}
