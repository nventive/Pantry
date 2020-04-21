using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Pantry.Continuation;

namespace Pantry.AspNetCore.Models
{
    /// <summary>
    /// Models that reflects <see cref="IContinuationEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    public class ContinuationEnumerableModel<T> : IContinuation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuationEnumerableModel{T}"/> class.
        /// Used for JSON support.
        /// </summary>
        public ContinuationEnumerableModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuationEnumerableModel{T}"/> class.
        /// </summary>
        /// <param name="continuationEnumerable">The <see cref="IContinuationEnumerable{T}"/>.</param>
        public ContinuationEnumerableModel(IContinuationEnumerable<T> continuationEnumerable)
        {
            Items = continuationEnumerable;
            ContinuationToken = continuationEnumerable?.ContinuationToken;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuationEnumerableModel{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="continuationToken">The continuation token.</param>
        public ContinuationEnumerableModel(IEnumerable<T> items, string? continuationToken)
        {
            Items = items;
            ContinuationToken = continuationToken;
        }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        [Description("The items in the collection.")]
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        /// <summary>
        /// Gets or sets the continuation token.
        /// </summary>
        [Description("The continuation token, if any. Pass it back to get the rest of the results.")]
        public string? ContinuationToken { get; set; }
    }
}
