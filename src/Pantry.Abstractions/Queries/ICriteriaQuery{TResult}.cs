using System.Collections.Generic;

namespace Pantry.Queries
{
    /// <summary>
    /// Type of query that supports <see cref="ICriterion"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    public interface ICriteriaQuery<TResult> : IQuery<TResult>, ICollection<ICriterion>
    {
    }
}
