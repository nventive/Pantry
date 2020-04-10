using Pantry.Continuation;

namespace Pantry.Queries
{
    /// <summary>
    /// Query for entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResult">The return type.</typeparam>
    public interface IQuery<TEntity, TResult> : IContinuation
    {
        /// <summary>
        /// Gets or sets the number of items to fetch.
        /// </summary>
        int Limit { get; set; }
    }
}
