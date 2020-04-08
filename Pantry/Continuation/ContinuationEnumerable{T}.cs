using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pantry.Continuation
{
    /// <summary>
    /// A decorator for <see cref="IEnumerable{T}"/> that provides a <see cref="IContinuationEnumerable{T}"/> implementation.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    public class ContinuationEnumerable<T> : IContinuationEnumerable<T>
    {
        private readonly IEnumerable<T> _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuationEnumerable{T}"/> class.
        /// </summary>
        /// <param name="inner">The original <see cref="IEnumerable{T}"/>.</param>
        /// <param name="continuationToken">The continuation token if any.</param>
        public ContinuationEnumerable(IEnumerable<T> inner, string? continuationToken = null)
        {
            _inner = inner ?? Enumerable.Empty<T>();
            ContinuationToken = continuationToken;
        }

        /// <inheritdoc/>
        public string? ContinuationToken { get; set; }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override string ToString()
        {
            var count = _inner.Count();
            var continuationToken = string.IsNullOrEmpty(ContinuationToken) ? "<no-ct>" : ContinuationToken;
            continuationToken = continuationToken.Length > 10 ? $"{continuationToken.Substring(0, 10)}..." : continuationToken;

            if (count == 0)
            {
                return $"[{nameof(ContinuationEnumerable)}<{typeof(T).Name}>]: (<empty>/{continuationToken})";
            }

            return $"[{nameof(ContinuationEnumerable)}<{typeof(T).Name}>]: ({_inner.Count()}/{continuationToken}): {_inner.First()}...";
        }
    }
}
