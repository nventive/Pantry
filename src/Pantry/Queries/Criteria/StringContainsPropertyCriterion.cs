using System.Linq;
using System.Linq.Dynamic.Core;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// Criterion that represents string contains..
    /// </summary>
    public class StringContainsPropertyCriterion : PropertyCriterion, IQueryableCriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringContainsPropertyCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The target value.</param>
        public StringContainsPropertyCriterion(string propertyPath, string value)
            : base(propertyPath, value)
        {
        }

        /// <inheritdoc/>
        public IQueryable Apply(IQueryable queryable) => queryable.Where($"{PropertyPath}.Contains(@0)", Value);

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {PropertyPath} contains \"{Value}\"";
    }
}
