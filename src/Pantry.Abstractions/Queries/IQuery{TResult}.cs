using Pantry.Continuation;

namespace Pantry.Queries
{
    /// <summary>
    /// Query for entities.
    /// </summary>
    /// <typeparam name="TResult">The entity type.</typeparam>
    public interface IQuery<TResult> : IContinuation
    {
        /// <summary>
        /// Gets or sets the number of items to fetch.
        /// </summary>
        int Limit { get; set; }
    }
}
