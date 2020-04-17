namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// Alows sorting of the criteria query results.
    /// </summary>
    public class OrderCriterion : PropertyCriterion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderCriterion"/> class.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="ascending">True to sort by ascending, false by descending.</param>
        public OrderCriterion(string propertyPath, bool ascending = true)
            : base(propertyPath)
        {
            Ascending = ascending;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the order should be ascending or descending.
        /// </summary>
        public bool Ascending { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}] {PropertyPath} {(Ascending ? "ASC" : "DESC")}";
    }
}
