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

        //public int? AgeEq
        //{
        //    get => this.EqualToValue(x => x.Age);
        //    set => this.EqualTo(x => x.Age, value);
        //}
    }
}
