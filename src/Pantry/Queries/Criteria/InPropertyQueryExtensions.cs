using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Pantry.Reflection;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// <see cref="ICriteriaQuery{TResult}"/> extension methods.
    /// </summary>
    public static class InPropertyQueryExtensions
    {
        /// <summary>
        /// Adds a criterion for property inclusion in a set.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="values">The values set.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> In<TResult>(this ICriteriaQuery<TResult> query, string propertyPath, IEnumerable<object>? values)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (values != null)
            {
                query.Add(new InPropertyCriterion(propertyPath, values));
            }

            return query;
        }

        /// <summary>
        /// Adds a criterion for property inclusion in a set.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TProperty">The type of property.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="values">The values set.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> In<TResult, TProperty>(
            this ICriteriaQuery<TResult> query,
            Expression<Func<TResult, TProperty>> propertyPath,
            IEnumerable<TProperty>? values)
            => query.In(PropertyVisitor.GetPropertyPath(propertyPath), values.Cast<object>());

        /// <summary>
        /// Finds the first set value for inclusion in a set of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="In{TResult}(ICriteriaQuery{TResult}, string, IEnumerable{object})"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static IEnumerable<TValue> InValue<TResult, TValue>(this ICriteriaQuery<TResult> query, string propertyPath)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return (IEnumerable<TValue>)query
                .OfType<InPropertyCriterion>()
                .FirstOrDefault(x => x.PropertyPath == propertyPath)?
                .Values!;
        }

        /// <summary>
        /// Finds the first set value for inclusion in a set of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="In{TResult, TProperty}(ICriteriaQuery{TResult}, Expression{Func{TResult, TProperty}}, IEnumerable{TProperty})"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static IEnumerable<TValue> InValue<TResult, TValue>(
            this ICriteriaQuery<TResult> query,
            Expression<Func<TResult, TValue>> propertyPath)
            => query.InValue<TResult, TValue>(PropertyVisitor.GetPropertyPath(propertyPath));
    }
}
