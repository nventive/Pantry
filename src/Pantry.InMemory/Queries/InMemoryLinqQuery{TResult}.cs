using System.Linq;
using Pantry.Queries;

namespace Pantry.InMemory.Queries
{
    /// <summary>
    /// Base class for in-memory queries using linq.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    public abstract class InMemoryLinqQuery<TResult> : Query<TResult>
    {
        /// <summary>
        /// Applies the query to the <paramref name="queryable"/> and returns the resulting <see cref="IQueryable{TResult}"/>.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable"/>.</param>
        /// <returns>The updated <see cref="IQueryable"/>.</returns>
        public abstract IQueryable<TResult> Apply(IQueryable<TResult> queryable);
    }
}
