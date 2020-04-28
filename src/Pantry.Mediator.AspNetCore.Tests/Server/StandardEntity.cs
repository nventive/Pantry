namespace Pantry.Mediator.AspNetCore.Tests.Server
{
    public class StandardEntity : RootAggregateEntity
    {
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
    }
}
