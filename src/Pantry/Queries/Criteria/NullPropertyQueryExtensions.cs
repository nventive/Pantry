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
    public static class NullPropertyQueryExtensions
    {
        /// <summary>
        /// Adds a criterion for property null.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="isNull">A value indicating whether to query for NULL (true) or NOT NULL (false) values.</param>
        /// <returns>The updated <see cref="ICriteriaRepositoryQuery{TResult}"/>.</returns>
        public static ICriteriaRepositoryQuery<TResult> IsNull<TResult>(this ICriteriaRepositoryQuery<TResult> query, string propertyPath, bool? isNull = true)
        {
            if (isNull.HasValue)
            {
                query.AddOrReplacePropertyCriterion(new NullPropertyCriterion(propertyPath, isNull.Value));
            }

            return query;
        }

        /// <summary>
        /// Adds a criterion for property null.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TProperty">The type of property.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="isNull">A value indicating whether to query for NULL (true) or NOT NULL (false) values.</param>
        /// <returns>The updated <see cref="ICriteriaRepositoryQuery{TResult}"/>.</returns>
        public static ICriteriaRepositoryQuery<TResult> IsNull<TResult, TProperty>(
            this ICriteriaRepositoryQuery<TResult> query,
            Expression<Func<TResult, TProperty>> propertyPath,
            bool? isNull = true)
            => query.IsNull(PropertyVisitor.GetPropertyPath(propertyPath), isNull);

        /// <summary>
        /// Finds the first set value for inequality comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="IsNull{TResult, TProperty}(ICriteriaRepositoryQuery{TResult}, Expression{Func{TResult, TProperty}}, bool?)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static bool? IsNullValue<TResult, TValue>(this ICriteriaRepositoryQuery<TResult> query, string propertyPath)
            => query.FirstOrDefaultPropertyCriterion<TResult, NullPropertyCriterion>(propertyPath)?.IsNull;

        /// <summary>
        /// Finds the first set value for inequality comparison of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="IsNull{TResult, TProperty}(ICriteriaRepositoryQuery{TResult}, Expression{Func{TResult, TProperty}}, bool?)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static bool? IsNullValue<TResult, TValue>(
            this ICriteriaRepositoryQuery<TResult> query,
            Expression<Func<TResult, TValue>> propertyPath)
            => query.IsNullValue<TResult, TValue>(PropertyVisitor.GetPropertyPath(propertyPath));
    }
}
