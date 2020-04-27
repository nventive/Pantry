using System;
using System.Linq;
using System.Linq.Expressions;
using Pantry.Queries.Criteria;
using Pantry.Reflection;

namespace Pantry.Queries
{
    /// <summary>
    /// <see cref="ICriteriaRepositoryQuery{TResult}"/> extension methods.
    /// </summary>
    public static class StringContainsPropertyQueryExtensions
    {
        /// <summary>
        /// Adds a criterion for string contains.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The equality value to compare to.</param>
        /// <returns>The updated <see cref="ICriteriaRepositoryQuery{TResult}"/>.</returns>
        public static ICriteriaRepositoryQuery<TResult> StringContains<TResult>(this ICriteriaRepositoryQuery<TResult> query, string propertyPath, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                query.AddOrReplacePropertyCriterion(new StringContainsPropertyCriterion(propertyPath, value));
            }

            return query;
        }

        /// <summary>
        /// Adds a criterion for property equality.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The equality value to compare to.</param>
        /// <returns>The updated <see cref="ICriteriaRepositoryQuery{TResult}"/>.</returns>
        public static ICriteriaRepositoryQuery<TResult> StringContains<TResult>(
            this ICriteriaRepositoryQuery<TResult> query,
            Expression<Func<TResult, string?>> propertyPath,
            string? value)
            => query.StringContains(PropertyVisitor.GetPropertyPath(propertyPath), value);

        /// <summary>
        /// Finds the first set value for equality comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="StringContains{TResult}(ICriteriaRepositoryQuery{TResult}, string, string?)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static TValue StringContainsValue<TResult, TValue>(this ICriteriaRepositoryQuery<TResult> query, string propertyPath)
            => (TValue)query.FirstOrDefaultPropertyCriterion<TResult, StringContainsPropertyCriterion>(propertyPath)?.Value!;

        /// <summary>
        /// Finds the first set value for equality comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="StringContains{TResult}(ICriteriaRepositoryQuery{TResult}, Expression{Func{TResult, string}}, string)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static TValue StringContainsValue<TResult, TValue>(
            this ICriteriaRepositoryQuery<TResult> query,
            Expression<Func<TResult, TValue>> propertyPath)
            => query.StringContainsValue<TResult, TValue>(PropertyVisitor.GetPropertyPath(propertyPath));
    }
}
