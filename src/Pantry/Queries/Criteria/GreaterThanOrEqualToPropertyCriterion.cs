using System.Linq;
using System.Linq.Dynamic.Core;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// Criterion that represents property comparison.
    /// </summary>
    public class GreaterThanOrEqualToPropertyCriterion : PropertyCriterion, IQueryableCriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GreaterThanOrEqualToPropertyCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The target value.</param>
        public GreaterThanOrEqualToPropertyCriterion(string propertyPath, object? value)
            : base(propertyPath, value)
        {
        }

        /// <inheritdoc/>
        public IQueryable Apply(IQueryable queryable) => queryable.Where($"{PropertyPath} >= @0", Value);

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {PropertyPath} >= {Value}";
    }
}
