using System;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// <see cref="ICriterion"/> that applies to a property path and a value.
    /// </summary>
    public class PropertyCriterion : ICriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The target value.</param>
        public PropertyCriterion(string propertyPath, object? value)
        {
            PropertyPath = propertyPath ?? throw new ArgumentNullException(nameof(propertyPath));
            Value = value;
        }

        /// <summary>
        /// Gets or sets the property path.
        /// </summary>
        public string PropertyPath { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object? Value { get; set; }
    }
}
