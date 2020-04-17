using System;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// <see cref="ICriterion"/> that applies to a property path and a value.
    /// </summary>
    public class PropertyValueCriterion : PropertyCriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The target value.</param>
        public PropertyValueCriterion(string propertyPath, object? value)
            : base(propertyPath)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object? Value { get; set; }
    }
}
