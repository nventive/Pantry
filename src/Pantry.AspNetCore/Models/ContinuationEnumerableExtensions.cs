using System;
using System.Linq;
using Pantry.Continuation;

namespace Pantry.AspNetCore.Models
{
    /// <summary>
    /// <see cref="IContinuationEnumerable{T}"/> extension methods.
    /// </summary>
    public static class ContinuationEnumerableExtensions
    {
        /// <summary>
        /// Converts a <see cref="IContinuationEnumerable{T}"/> to a <see cref="ContinuationEnumerableModel{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The items type.</typeparam>
        /// <param name="source">The source <see cref="IContinuationEnumerable{T}"/>.</param>
        /// <returns>The <see cref="ContinuationEnumerableModel{T}"/>.</returns>
        public static ContinuationEnumerableModel<TSource> ToModel<TSource>(this IContinuationEnumerable<TSource> source)
        {
            return new ContinuationEnumerableModel<TSource>(source);
        }

        /// <summary>
        /// Converts a <see cref="IContinuationEnumerable{T}"/> to a <see cref="ContinuationEnumerableModel{T}"/>
        /// by projecting items to models.
        /// </summary>
        /// <typeparam name="TSource">The items type.</typeparam>
        /// <typeparam name="TProjected">The model items type.</typeparam>
        /// <param name="source">The source <see cref="IContinuationEnumerable{T}"/>.</param>
        /// <param name="select">The projection method.</param>
        /// <returns>The <see cref="ContinuationEnumerableModel{T}"/>.</returns>
        public static ContinuationEnumerableModel<TProjected> ToModel<TSource, TProjected>(this IContinuationEnumerable<TSource> source, Func<TSource, TProjected> select)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new ContinuationEnumerableModel<TProjected>(source.Select(select), source.ContinuationToken);
        }
    }
}
