using System;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// <see cref="ICriterion"/> that applies to a property path.
    /// </summary>
    public class PropertyCriterion : ICriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        public PropertyCriterion(string propertyPath)
        {
            PropertyPath = propertyPath ?? throw new ArgumentNullException(nameof(propertyPath));
        }

        /// <summary>
        /// Gets or sets the property path.
        /// </summary>
        public string PropertyPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="PropertyPath"/> contains a sub-path (e.g. Prop1.Prop2).
        /// </summary>
        public bool PropertyPathContainsSubPath => PropertyPath.Contains(".", StringComparison.Ordinal);
    }
}
