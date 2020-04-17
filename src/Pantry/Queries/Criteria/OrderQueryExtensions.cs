﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Pantry.Reflection;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// <see cref="ICriteriaQuery{TResult}"/> extension methods.
    /// </summary>
    public static class OrderQueryExtensions
    {
        /// <summary>
        /// Adds a criterion for ordering.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="ascending">True to order by ASC, false for DESC.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> OrderBy<TResult>(this ICriteriaQuery<TResult> query, string propertyPath, bool ascending = true)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            query.Add(new OrderCriterion(propertyPath, ascending));

            return query;
        }

        /// <summary>
        /// Adds a criterion for ordering.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPathAndAscending">The property path. Use the prefix "+" to sort by ASC or "-" to sort by DESC. Defaults to ASC.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> OrderByPathWithAscendingIndicator<TResult>(this ICriteriaQuery<TResult> query, string? propertyPathAndAscending)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (!string.IsNullOrEmpty(propertyPathAndAscending))
            {
                var firstLetter = propertyPathAndAscending[0];
                switch (firstLetter)
                {
                    case '+':
                        query.Add(new OrderCriterion(propertyPathAndAscending.Substring(1), true));
                        break;
                    case '-':
                        query.Add(new OrderCriterion(propertyPathAndAscending.Substring(1), false));
                        break;
                    default:
                        query.Add(new OrderCriterion(propertyPathAndAscending));
                        break;
                }
            }

            return query;
        }

        /// <summary>
        /// Adds a criterion for ordering.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <typeparam name="TProperty">The type of property.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="ascending">True to order by ASC, false for DESC.</param>
        /// <returns>The updated <see cref="ICriteriaQuery{TResult}"/>.</returns>
        public static ICriteriaQuery<TResult> OrderBy<TResult, TProperty>(
            this ICriteriaQuery<TResult> query,
            Expression<Func<TResult, TProperty>> propertyPath,
            bool ascending)
            => query.OrderBy(PropertyVisitor.GetPropertyPath(propertyPath), ascending);

        /// <summary>
        /// Finds the first order criterion and returns it as Property path and ascending prefix.
        /// This is the "inverse" of <see cref="OrderByPathWithAscendingIndicator{TResult}(ICriteriaQuery{TResult}, string)"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result for the query.</typeparam>
        /// <param name="query">The <see cref="ICriteriaQuery{TResult}"/>.</param>
        /// <returns>The property path with the ascending prefix, if found, or null if not found.</returns>
        public static string? OrderByPathAndAscendingIndicatorValue<TResult>(this ICriteriaQuery<TResult> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var orderCriterion = query
                .OfType<OrderCriterion>()
                .FirstOrDefault();

            return orderCriterion is null ? null : $"{(orderCriterion.Ascending ? "+" : "-")}{orderCriterion.PropertyPath}";
        }
    }
}