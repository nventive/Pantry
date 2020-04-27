namespace Pantry.Queries
{
    /// <summary>
    /// Base class for <see cref="IRepositoryQuery{TResult}"/> implementation.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    public abstract class RepositoryQuery<TResult> : IRepositoryQuery<TResult>
    {
        /// <inheritdoc />
        public int Limit { get; set; } = RepositoryQuery.DefaultLimit;

        /// <inheritdoc />
        public string? ContinuationToken { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"[{GetType().Name}]: (ct: {ContinuationToken ?? "<no-ct>"}, limit: {Limit})";
    }
}
