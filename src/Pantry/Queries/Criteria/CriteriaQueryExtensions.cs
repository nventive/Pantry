using System;
using System.Linq;

namespace Pantry.Queries.Criteria
{
    /// <summary>
    /// <see cref="ICriteriaRepositoryQuery{TResult}"/> extension methods.
    /// </summary>
    public static class CriteriaQueryExtensions
    {
        /// <summary>
        /// Returns the first <see cref="PropertyCriterion"/> of type <typeparamref name="TCriterion"/>
        /// which mathces the <paramref name="propertyPath"/> as well.
        /// </summary>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <typeparam name="TCriterion">The criterion type.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="propertyPath">The proeprty path to search for.</param>
        /// <returns>The first criterion, or null if not found.</returns>
        public static TCriterion? FirstOrDefaultPropertyCriterion<TResult, TCriterion>(this ICriteriaRepositoryQuery<TResult> query, string propertyPath)
            where TCriterion : PropertyCriterion
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query
                .GetCriterions()
                .OfType<TCriterion>()
                .FirstOrDefault(x => x.PropertyPath == propertyPath);
        }

        /// <summary>
        /// Adds the <paramref name="criterion"/>, or replaces it based on the same type and property path.
        /// </summary>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <typeparam name="TCriterion">The criterion type.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="criterion">The criterion to add or replace.</param>
        /// <returns>The first criterion, or null if not found.</returns>
        public static ICriteriaRepositoryQuery<TResult> AddOrReplacePropertyCriterion<TResult, TCriterion>(
            this ICriteriaRepositoryQuery<TResult> query,
            TCriterion criterion)
            where TCriterion : PropertyCriterion
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (criterion is null)
            {
                throw new ArgumentNullException(nameof(criterion));
            }

            var existing = query.FirstOrDefaultPropertyCriterion<TResult, TCriterion>(criterion.PropertyPath);
            if (existing != null)
            {
                query.Remove(existing);
            }

            query.Add(criterion);

            return query;
        }
    }
}
