﻿using System.Collections.Generic;

namespace Pantry.Queries
{
    /// <summary>
    /// Type of query that supports <see cref="ICriterion"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    public interface ICriteriaRepositoryQuery<TResult> : IRepositoryQuery<TResult>
    {
        /// <summary>
        /// Gets the <see cref="ICriterion"/>.
        /// </summary>
        /// <returns>The <see cref="ICriterion"/>.</returns>
        IEnumerable<ICriterion> GetCriterions();

        /// <summary>
        /// Adds criterion.
        /// </summary>
        /// <param name="criterions">The <see cref="ICriterion"/> to add.</param>
        void Add(params ICriterion[] criterions);

        /// <summary>
        /// Removes a specific criterion.
        /// </summary>
        /// <param name="criterion">The <see cref="ICriterion"/> to remove.</param>
        void Remove(ICriterion criterion);
    }
}
