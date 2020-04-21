namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// Criterion that represents property equality.
    /// </summary>
    public class NotEqualToPropertyCriterion : PropertyValueCriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotEqualToPropertyCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The target value.</param>
        public NotEqualToPropertyCriterion(string propertyPath, object? value)
            : base(propertyPath, value)
        {
        }

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {PropertyPath} != {Value}";
    }
}
