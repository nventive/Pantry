using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Pantry.Decorators;

namespace Pantry.FluentValidation
{
    /// <summary>
    /// <see cref="IRepository{TEntity}"/> decorator that uses FluentValidation to perform
    /// entity validation before repository insertion.
    /// Uses FluentValidation rulesets to indicate what to run.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class FluentValidationRepositoryDecorator<TEntity> : RepositoryDecorator<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidationRepositoryDecorator{TEntity}"/> class.
        /// </summary>
        /// <param name="validator">The <see cref="IValidator"/>.</param>
        /// <param name="innerRepository">The inner repository.</param>
        /// <param name="includeDefaultRuleSet">true to include the default RuleSet when validating, false otherwise.</param>
        public FluentValidationRepositoryDecorator(
            IValidator<TEntity> validator,
            object innerRepository,
            bool includeDefaultRuleSet = true)
            : base(innerRepository)
        {
            Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            IncludeDefaultRuleSet = includeDefaultRuleSet;
        }

        /// <summary>
        /// Gets the validator.
        /// </summary>
        protected IValidator<TEntity> Validator { get; }

        /// <summary>
        /// Gets a value indicating whether to include the default RuleSet when validating.
        /// </summary>
        protected bool IncludeDefaultRuleSet { get; }

        /// <inheritdoc/>
        public override async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var ruleSet = IncludeDefaultRuleSet
                ? $"default,{RuleSets.Add}"
                : RuleSets.Add;
            await Validator.ValidateAndThrowAsync(
                entity,
                ruleSet: ruleSet,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            return await base.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task<(TEntity, bool)> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var ruleSet = IncludeDefaultRuleSet
                ? $"default,{RuleSets.Add},{RuleSets.Update}"
                : $"{RuleSets.Add},{RuleSets.Update}";
            await Validator.ValidateAndThrowAsync(
                entity,
                ruleSet: ruleSet,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            return await base.AddOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var ruleSet = IncludeDefaultRuleSet
                ? $"default,{RuleSets.Update}"
                : RuleSets.Update;
            await Validator.ValidateAndThrowAsync(
                entity,
                ruleSet: ruleSet,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            return await base.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        }
    }
}
