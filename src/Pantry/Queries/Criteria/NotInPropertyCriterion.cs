using System.Collections.Generic;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// Criterion that represents property exclusion from a set.
    /// </summary>
    public class NotInPropertyCriterion : PropertyCriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotInPropertyCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="values">The target values set.</param>
        public NotInPropertyCriterion(string propertyPath, IEnumerable<object> values)
            : base(propertyPath)
        {
            Values = values;
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        public IEnumerable<object> Values { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {PropertyPath} NOT IN ({string.Join(", ", Values)})";
    }
}
