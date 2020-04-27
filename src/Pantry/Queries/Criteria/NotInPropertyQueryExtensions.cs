using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Pantry.Queries.Criteria;
using Pantry.Reflection;

namespace Pantry.Queries
{
    /// <summary>
    /// <see cref="ICriteriaRepositoryQuery{TResult}"/> extension methods.
    /// </summary>
    public static class NotInPropertyQueryExtensions
    {
        /// <summary>
        /// Adds a criterion for property exclusion from a set.
        /// Will add to any existing list and de-duplicates.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="values">The values set.</param>
        /// <returns>The updated <see cref="ICriteriaRepositoryQuery{TResult}"/>.</returns>
        public static ICriteriaRepositoryQuery<TResult> NotIn<TResult>(this ICriteriaRepositoryQuery<TResult> query, string propertyPath, IEnumerable<object>? values)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (values != null && values.Any())
            {
                var existing = query.FirstOrDefaultPropertyCriterion<TResult, NotInPropertyCriterion>(propertyPath);
                if (existing is null)
                {
                    query.Add(new NotInPropertyCriterion(propertyPath, values));
                }
                else
                {
                    existing.Values = existing.Values.Concat(values).Distinct().ToList();
                }
            }

            return query;
        }

        /// <summary>
        /// Adds a criterion for property exclusion from a set.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TProperty">The type of property.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="values">The values set.</param>
        /// <returns>The updated <see cref="ICriteriaRepositoryQuery{TResult}"/>.</returns>
        public static ICriteriaRepositoryQuery<TResult> NotIn<TResult, TProperty>(
            this ICriteriaRepositoryQuery<TResult> query,
            Expression<Func<TResult, TProperty>> propertyPath,
            IEnumerable<TProperty>? values)
            => query.NotIn(PropertyVisitor.GetPropertyPath(propertyPath), values.Cast<object>());

        /// <summary>
        /// Finds the first set value for exclusion from a set of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="NotIn{TResult}(ICriteriaRepositoryQuery{TResult}, string, IEnumerable{object})"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static IEnumerable<TValue> NotInValue<TResult, TValue>(this ICriteriaRepositoryQuery<TResult> query, string propertyPath)
            => (IEnumerable<TValue>)query.FirstOrDefaultPropertyCriterion<TResult, NotInPropertyCriterion>(propertyPath)?.Values!;

        /// <summary>
        /// Finds the first set value for exclusion from a set of <paramref name="propertyPath"/>.
        /// This is the "inverse" of <see cref="NotIn{TResult, TProperty}(ICriteriaRepositoryQuery{TResult}, Expression{Func{TResult, TProperty}}, IEnumerable{TProperty})"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="query">The <see cref="ICriteriaRepositoryQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The found value, or default if not found.</returns>
        public static IEnumerable<TValue> NotInValue<TResult, TValue>(
            this ICriteriaRepositoryQuery<TResult> query,
            Expression<Func<TResult, TValue>> propertyPath)
            => query.NotInValue<TResult, TValue>(PropertyVisitor.GetPropertyPath(propertyPath));
    }
}
