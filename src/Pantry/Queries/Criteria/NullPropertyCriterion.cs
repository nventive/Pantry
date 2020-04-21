namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// Criterion that represents property null.
    /// </summary>
    public class NullPropertyCriterion : PropertyCriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullPropertyCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="isNull">A value indicating whether to query for NULL (true) or NOT NULL (false) values.</param>
        public NullPropertyCriterion(string propertyPath, bool isNull = true)
            : base(propertyPath)
        {
            IsNull = isNull;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to query for NULL (true) or NOT NULL (false) values.
        /// </summary>
        public bool IsNull { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {PropertyPath} is {(IsNull ? string.Empty : "not ")}null";
    }
}
