namespace Pantry.Mediator.AspNetCore.Execution
{
    /// <summary>
    /// Options when performing <see cref="IDomainRequest"/> RestApi execution.
    /// </summary>
    public class DomainRequestExecutionOptions
    {
        /// <summary>
        /// Gets or sets the CreatedAtRedirect url pattern, if any.
        /// </summary>
        public string? CreatedAtRedirectPattern { get; set; }
    }
}
