namespace Pantry.Queries
{
    /// <summary>
    /// Base class for <see cref="IQuery{TResult}"/> implementation.
    /// </summary>
    /// <typeparam name="TResult">The result element types.</typeparam>
    public abstract class Query<TResult> : IQuery<TResult>
    {
        /// <summary>
        /// Gets the default limit to apply when none provided.
        /// </summary>
        public static readonly int DefaultLimit = 50;

        /// <inheritdoc />
        public int Limit { get; set; } = DefaultLimit;

        /// <inheritdoc />
        public string? ContinuationToken { get; set; }
    }
}
