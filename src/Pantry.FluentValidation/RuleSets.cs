using System.Threading;
using Pantry.Traits;

namespace Pantry.FluentValidation
{
    /// <summary>
    /// Holds static values for RuleSets.
    /// </summary>
    public static class RuleSets
    {
        /// <summary>
        /// Gets the RuleSet for the <see cref="IRepositoryAdd{TEntity}.AddAsync(TEntity, CancellationToken)"/> operation.
        /// </summary>
        public const string Add = "Add";

        /// <summary>
        /// Gets the RuleSet for the <see cref="IRepositoryUpdate{TEntity}.UpdateAsync(TEntity, CancellationToken)"/> operation.
        /// </summary>
        public const string Update = "Update";
    }
}
