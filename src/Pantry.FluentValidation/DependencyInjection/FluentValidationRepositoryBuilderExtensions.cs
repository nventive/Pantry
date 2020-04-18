using System;
using FluentValidation;
using Pantry;
using Pantry.DependencyInjection;
using Pantry.FluentValidation;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder{TEntity}"/> extension methods.
    /// </summary>
    public static class FluentValidationRepositoryBuilderExtensions
    {
        /// <summary>
        /// Decorates the repository to add validation perform by FluentValidation.
        /// Remeber to register your <see cref="IValidator{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="includeDefaultRuleSet">true to include the default RuleSet when validating, false otherwise.</param>
        /// <returns>The updated <see cref="IRepositoryBuilder{TEntity}"/>.</returns>
        public static IRepositoryBuilder<TEntity> WithFluentValidation<TEntity>(
            this IRepositoryBuilder<TEntity> builder,
            bool includeDefaultRuleSet = true)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddRepositoryDecorator(
                (repo, sp) => new FluentValidationRepositoryDecorator<TEntity>(
                    sp.GetRequiredService<IValidator<TEntity>>(),
                    repo,
                    includeDefaultRuleSet));

            return builder;
        }
    }
}
