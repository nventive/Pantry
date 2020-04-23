using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Pantry.AspNetCore.Models
{
    /// <summary>
    /// Model for resource collection results.
    /// </summary>
    /// <typeparam name="TAttributes">The resource attributes.</typeparam>
    public class ResourceCollection<TAttributes>
        where TAttributes : class
    {
        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        [Description("The items in the collection.")]
        public IEnumerable<ResourceCollectionItem<TAttributes>> Items { get; set; } = Enumerable.Empty<ResourceCollectionItem<TAttributes>>();

        /// <summary>
        /// Gets or sets the continuation token.
        /// </summary>
        [Description("The continuation token, if any. Pass it back to get the rest of the results.")]
        public string? ContinuationToken { get; set; }
    }
}
