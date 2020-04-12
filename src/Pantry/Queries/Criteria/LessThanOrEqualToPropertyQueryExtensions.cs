using System;
using System.Linq;
using System.Linq.Expressions;
using Pantry.Utils;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// <see cref="ICriteriaQuery{TResult}"/> extension methods.
    /// </summary>
    public static class LessThanOrEqualToPropertyQueryExtensions
    {
        /// <summary>
        /// Adds a criterion for property value less than or equal to.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The equality value to compare to.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> LessThanOrEqualTo<TResult>(this ICriteriaQuery<TResult> query, string propertyPath, object? value)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            query.Add(new LessThanOrEqualToPropertyCriterion(propertyPath, value));

            return query;
        }

        /// <summary>
        /// Adds a criterion for property value less than or equal to.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TProperty">The type of property.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The equality value to compare to.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> LessThanOrEqualTo<TResult, TProperty>(
            this ICriteriaQuery<TResult> query,
            Expression<Func<TResult, TProperty>> propertyPath,
            TProperty value)
            => query.LessThanOrEqualTo(PropertyVisitor.GetPropertyPath(propertyPath), value);

        /// <summary>
        /// Finds the first set value for value less than or equal to comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="LessThanOrEqualTo{TResult}(ICriteriaQuery{TResult}, string, object?)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static TValue LessThanOrEqualToValue<TResult, TValue>(this ICriteriaQuery<TResult> query, string propertyPath)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return (TValue)query
                .OfType<LessThanOrEqualToPropertyCriterion>()
                .FirstOrDefault(x => x.PropertyPath == propertyPath)?
                .Value!;
        }

        /// <summary>
        /// Finds the first set value for value less than or equal to comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="LessThanOrEqualTo{TResult, TProperty}(ICriteriaQuery{TResult}, Expression{Func{TResult, TProperty}}, TProperty)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static TValue LessThanOrEqualToValue<TResult, TValue>(
            this ICriteriaQuery<TResult> query,
            Expression<Func<TResult, TValue>> propertyPath)
            => query.LessThanOrEqualToValue<TResult, TValue>(PropertyVisitor.GetPropertyPath(propertyPath));
    }
}
