namespace Pantry.Continuation
{
    /// <summary>
    /// Indicates that it contains a continuation token.
    /// </summary>
    public interface IContinuation
    {
        /// <summary>
        /// Gets or sets the continuation token.
        /// </summary>
        string? ContinuationToken { get; set; }
    }
}
