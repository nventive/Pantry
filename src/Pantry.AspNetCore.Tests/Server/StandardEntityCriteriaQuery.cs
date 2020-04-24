using System.Collections.Generic;
using Pantry.Queries;

namespace Pantry.AspNetCore.Tests.Server
{
    public class StandardEntityCriteriaQuery : CriteriaQuery<StandardEntity>
    {
        public string? IdEq
        {
            get => this.EqualToValue(x => x.Id);
            set => this.EqualTo(x => x.Id, value);
        }

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

        public IEnumerable<int>? AgeIn
        {
            get => this.InValue(x => x.Age);
            set => this.In(x => x.Age, value);
        }

        public string? OrderBy
        {
            get => this.OrderByPathAndAscendingIndicatorValue();
            set => this.OrderByPathWithAscendingIndicator(value);
        }
    }
}
