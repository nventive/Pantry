namespace Pantry.FluentValidation.Tests
{
    public class RefEntity : RootAggregateEntity
    {
        /// <summary>
        /// Gets or sets a reference to a <see cref="StandardEntity"/>.
        /// </summary>
        public string? StandardEntityId { get; set; }
    }
}
