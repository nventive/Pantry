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
    public static class LessThanOrEqualToPropertyQueryExtensions
    {
        /// <summary>
        /// Adds a criterion for property value less than or equal to.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The equality value to compare to.</param>
        /// <returns>The updated <see cref="ICriteriaRepositoryQuery{TResult}"/>.</returns>
        public static ICriteriaRepositoryQuery<TResult> LessThanOrEqualTo<TResult>(this ICriteriaRepositoryQuery<TResult> query, string propertyPath, object? value)
            => query.AddOrReplacePropertyCriterion(new LessThanOrEqualToPropertyCriterion(propertyPath, value));

        /// <summary>
        /// Adds a criterion for property value less than or equal to.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TProperty">The type of property.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="value">The equality value to compare to.</param>
        /// <returns>The updated <see cref="ICriteriaRepositoryQuery{TResult}"/>.</returns>
        public static ICriteriaRepositoryQuery<TResult> LessThanOrEqualTo<TResult, TProperty>(
            this ICriteriaRepositoryQuery<TResult> query,
            Expression<Func<TResult, TProperty>> propertyPath,
            TProperty value)
            => query.LessThanOrEqualTo(PropertyVisitor.GetPropertyPath(propertyPath), value);

        /// <summary>
        /// Finds the first set value for value less than or equal to comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="LessThanOrEqualTo{TResult}(ICriteriaRepositoryQuery{TResult}, string, object?)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static TValue LessThanOrEqualToValue<TResult, TValue>(this ICriteriaRepositoryQuery<TResult> query, string propertyPath)
            => (TValue)query.FirstOrDefaultPropertyCriterion<TResult, LessThanOrEqualToPropertyCriterion>(propertyPath)?.Value!;

        /// <summary>
        /// Finds the first set value for value less than or equal to comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="LessThanOrEqualTo{TResult, TProperty}(ICriteriaRepositoryQuery{TResult}, Expression{Func{TResult, TProperty}}, TProperty)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static TValue LessThanOrEqualToValue<TResult, TValue>(
            this ICriteriaRepositoryQuery<TResult> query,
            Expression<Func<TResult, TValue>> propertyPath)
            => query.LessThanOrEqualToValue<TResult, TValue>(PropertyVisitor.GetPropertyPath(propertyPath));
    }
}
