using System.Linq;
using System.Linq.Dynamic.Core;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// Criterion that represents property comparison.
    /// </summary>
    public class LessThanOrEqualToPropertyCriterion : PropertyCriterion, IQueryableCriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanOrEqualToPropertyCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The target value.</param>
        public LessThanOrEqualToPropertyCriterion(string propertyPath, object? value)
            : base(propertyPath, value)
        {
        }

        /// <inheritdoc/>
        public IQueryable Apply(IQueryable queryable) => queryable.Where($"{PropertyPath} <= @0", Value);

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {PropertyPath} <= {Value}";
    }
}
