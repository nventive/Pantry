namespace Pantry.Queries
{
    /// <summary>
    /// Base class for <see cref="IQuery{TEntity, TResult}"/> implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResult">The return type.</typeparam>
    public abstract class Query<TEntity, TResult> : IQuery<TEntity, TResult>
    {
        /// <inheritdoc />
        public int Limit { get; set; } = Query.DefaultLimit;

        /// <inheritdoc />
        public string? ContinuationToken { get; set; }
    }
}
