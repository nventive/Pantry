using System.Collections.Generic;
using Pantry.Mediator.Repositories.Queries;
using Pantry.Queries;

namespace Pantry.Mediator.AspNetCore.Tests.Server
{
    public class FindStandardEntityQuery : FindByCriteriaDomainQuery<StandardEntity>
    {
        public string? NameContains
        {
            get => this.StringContainsValue(x => x.Name);
            set => this.StringContains(x => x.Name, value);
        }

        public string? NameEq
        {
            get => this.EqualToValue(x => x.Name);
            set => this.EqualTo(x => x.Name, value);
        }

        public IEnumerable<string> IdIn
        {
            get => this.InValue(x => x.Id);
            set => this.In(x => x.Id, value);
        }

        //public int? AgeEq
        //{
        //    get => this.EqualToValue(x => x.Age);
        //    set => this.EqualTo(x => x.Age, value);
        //}
    }
}
