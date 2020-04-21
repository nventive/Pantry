using System;
using System.Linq;
using System.Linq.Expressions;
using Pantry.Reflection;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// <see cref="ICriteriaQuery{TResult}"/> extension methods.
    /// </summary>
    public static class GreaterThanOrEqualToPropertyQueryExtensions
    {
        /// <summary>
        /// Adds a criterion for property value greater than or equal to.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The equality value to compare to.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> GreaterThanOrEqualTo<TResult>(this ICriteriaQuery<TResult> query, string propertyPath, object? value)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            query.AddCriterions(new GreaterThanOrEqualToPropertyCriterion(propertyPath, value));

            return query;
        }

        /// <summary>
        /// Adds a criterion for property value greater than or equal to.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TProperty">The type of property.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The equality value to compare to.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> GreaterThanOrEqualTo<TResult, TProperty>(
            this ICriteriaQuery<TResult> query,
            Expression<Func<TResult, TProperty>> propertyPath,
            TProperty value)
            => query.GreaterThanOrEqualTo(PropertyVisitor.GetPropertyPath(propertyPath), value);

        /// <summary>
        /// Finds the first set value for value greater than or equal to comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="GreaterThanOrEqualTo{TResult}(ICriteriaQuery{TResult}, string, object?)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static TValue GreaterThanOrEqualToValue<TResult, TValue>(this ICriteriaQuery<TResult> query, string propertyPath)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return (TValue)query
                .GetCriterions()
                .OfType<GreaterThanOrEqualToPropertyCriterion>()
                .FirstOrDefault(x => x.PropertyPath == propertyPath)?
                .Value!;
        }

        /// <summary>
        /// Finds the first set value for value greater than or equal to comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="GreaterThanOrEqualTo{TResult, TProperty}(ICriteriaQuery{TResult}, Expression{Func{TResult, TProperty}}, TProperty)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static TValue GreaterThanOrEqualToValue<TResult, TValue>(
            this ICriteriaQuery<TResult> query,
            Expression<Func<TResult, TValue>> propertyPath)
            => query.GreaterThanOrEqualToValue<TResult, TValue>(PropertyVisitor.GetPropertyPath(propertyPath));
    }
}
