using System;
using Pantry.Queries;

namespace Pantry.Tests.StandardTestSupport
{
    public class StandardEntityCriteriaQuery : CriteriaQuery<StandardEntity>
    {
        public string? NameEq
        {
            get => this.EqValue(x => x.Name);
            set => this.Eq(x => x.Name, value);
        }

        public int? AgeEq
        {
            get => this.EqValue(x => x.Age);
            set => this.Eq(x => x.Age, value);
        }

        public int? AgeGt
        {
            get => this.GtValue(x => x.Age);
            set => this.Gt(x => x.Age, value);
        }

        public DateTimeOffset? NotarizedAtEq
        {
            get => this.EqValue(x => x.NotarizedAt);
            set => this.Eq(x => x.NotarizedAt, value);
        }

        public DateTimeOffset? NotarizedAtGt
        {
            get => this.GtValue(x => x.NotarizedAt);
            set => this.Gt(x => x.NotarizedAt, value);
        }

        public string? RelatedNameEq
        {
            get => this.EqValue(x => x.Related!.Name);
            set => this.Eq(x => x.Related!.Name, value);
        }
    }
}
