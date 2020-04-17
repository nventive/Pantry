namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// Criterion that represents property comparison.
    /// </summary>
    public class LessThanOrEqualToPropertyCriterion : PropertyValueCriterion
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
        public override string ToString() => $"[{GetType().Name}] {PropertyPath} <= {Value}";
    }
}
