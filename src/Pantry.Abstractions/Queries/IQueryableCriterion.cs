using System.Linq;

namespace Pantry.Queries
{
    /// <summary>
    /// <see cref="ICriterion"/> that have a Linq translation possible.
    /// </summary>
    public interface IQueryableCriterion : ICriterion
    {
        /// <summary>
        /// Applies the criterion.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable"/>.</param>
        /// <returns>The updated <see cref="IQueryable"/>.</returns>
        IQueryable Apply(IQueryable queryable);
    }
}
