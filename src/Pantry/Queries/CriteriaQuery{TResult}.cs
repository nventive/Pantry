using System.Collections.Generic;

namespace Pantry.Queries
{
    /// <summary>
    /// <see cref="ICriteriaQuery{TResult}"/> implementation.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    public class CriteriaQuery<TResult> : ICriteriaQuery<TResult>
    {
        private readonly List<ICriterion> _criterions = new List<ICriterion>();

        /// <inheritdoc/>
        public int Limit { get; set; } = Query.DefaultLimit;

        /// <inheritdoc/>
        public string? ContinuationToken { get; set; }

        /// <inheritdoc/>
        public void Add(params ICriterion[] criterions) => _criterions.AddRange(criterions);

        /// <inheritdoc/>
        public IEnumerable<ICriterion> GetCriterions() => _criterions;

        /// <inheritdoc/>
        public void Remove(ICriterion criterion) => _criterions.Remove(criterion);

        /// <inheritdoc/>
        public override string ToString() => $"[{GetType().Name}]: {string.Join(", ", _criterions)}";
    }
}
