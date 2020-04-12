namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// Criterion that represents property equality.
    /// </summary>
    public class EqualPropertyCriterion : PropertyCriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EqualPropertyCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The target value.</param>
        public EqualPropertyCriterion(string propertyPath, object? value)
            : base(propertyPath, value)
        {
        }
    }
}
