namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// Criterion that represents property comparison.
    /// </summary>
    public class GreaterThanOrEqualToPropertyCriterion : PropertyValueCriterion
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
        public override string ToString() => $"[{GetType().Name}] {PropertyPath} >= {Value}";
    }
}
