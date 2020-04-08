using System.Collections.Generic;
using System.Linq;

namespace Pantry.Continuation
{
    /// <summary>
    /// A decorator for <see cref="IEnumerable{T}"/> that provides a <see cref="IContinuationEnumerable{T}"/> implementation.
    /// </summary>
    public static class ContinuationEnumerable
    {
        /// <summary>
        /// Returns an empty <see cref="IContinuationEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of objects to enumerate.</typeparam>
        /// <returns>The <see cref="IContinuationEnumerable{T}"/>.</returns>
        public static IContinuationEnumerable<T> Empty<T>() => new ContinuationEnumerable<T>(Enumerable.Empty<T>());
    }
}
