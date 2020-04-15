using System;
using System.Linq;
using System.Linq.Expressions;
using Pantry.Queries.Criteria;
using Pantry.Reflection;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// <see cref="ICriteriaQuery{TResult}"/> extension methods.
    /// </summary>
    public static class StringContainsPropertyQueryExtensions
    {
        /// <summary>
        /// Adds a criterion for string contains.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The equality value to compare to.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> StringContains<TResult>(this ICriteriaQuery<TResult> query, string propertyPath, string? value)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (!string.IsNullOrEmpty(value))
            {
                query.Add(new StringContainsPropertyCriterion(propertyPath, value));
            }

            return query;
        }

        /// <summary>
        /// Adds a criterion for property equality.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The equality value to compare to.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> StringContains<TResult>(
            this ICriteriaQuery<TResult> query,
            Expression<Func<TResult, string?>> propertyPath,
            string? value)
            => query.StringContains(PropertyVisitor.GetPropertyPath(propertyPath), value);

        /// <summary>
        /// Finds the first set value for equality comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="StringContains{TResult}(ICriteriaQuery{TResult}, string, string?)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static TValue StringContainsValue<TResult, TValue>(this ICriteriaQuery<TResult> query, string propertyPath)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return (TValue)query
                .OfType<StringContainsPropertyCriterion>()
                .FirstOrDefault(x => x.PropertyPath == propertyPath)?
                .Value!;
        }

        /// <summary>
        /// Finds the first set value for equality comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="StringContains{TResult}(ICriteriaQuery{TResult}, Expression{Func{TResult, string}}, string)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static TValue StringContainsValue<TResult, TValue>(
            this ICriteriaQuery<TResult> query,
            Expression<Func<TResult, TValue>> propertyPath)
            => query.StringContainsValue<TResult, TValue>(PropertyVisitor.GetPropertyPath(propertyPath));
    }
}
