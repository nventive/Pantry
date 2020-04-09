namespace Pantry.Queries
{
    /// <summary>
    /// Base class for <see cref="IQuery{TResult}"/> implementation.
    /// </summary>
    /// <typeparam name="TResult">The result element types.</typeparam>
    public abstract class Query<TResult> : IQuery<TResult>
    {
        /// <inheritdoc />
        public int Limit { get; set; } = Query.DefaultLimit;

        /// <inheritdoc />
        public string? ContinuationToken { get; set; }
    }
}
