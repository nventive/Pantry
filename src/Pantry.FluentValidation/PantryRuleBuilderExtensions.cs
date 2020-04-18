using System;
using Microsoft.Extensions.DependencyInjection;
using Pantry;
using Pantry.FluentValidation.Validators;
using Pantry.Traits;

namespace FluentValidation
{
    /// <summary>
    /// <see cref="IRuleBuilder{T, TProperty}"/> extensions.
    /// </summary>
    public static class PantryRuleBuilderExtensions
    {
#nullable disable
        /// <summary>
        /// Validates that the property is a valid Id for another entity.
        /// Allows validation of root entities references.
        /// </summary>
        /// <typeparam name="T">The base object validated.</typeparam>
        /// <typeparam name="TTargetEntity">The type of entity to check for by Id.</typeparam>
        /// <param name="ruleBuilder">The <see cref="IRuleBuilder{T, TProperty}"/>.</param>
        /// <param name="repoGetTargetEntity">The target entity repository.</param>
        /// <returns>The updated <see cref="IRuleBuilder{T, TProperty}"/>.</returns>
        public static IRuleBuilderOptions<T, string> ReferencesById<T, TTargetEntity>(
            this IRuleBuilder<T, string> ruleBuilder,
            IRepositoryGet<TTargetEntity> repoGetTargetEntity)
            where TTargetEntity : class, IIdentifiable
        {
            if (ruleBuilder is null)
            {
                throw new ArgumentNullException(nameof(ruleBuilder));
            }

            if (repoGetTargetEntity is null)
            {
                throw new ArgumentNullException(nameof(repoGetTargetEntity));
            }

            return ruleBuilder.SetValidator(new ReferencesByIdValidator<TTargetEntity>(repoGetTargetEntity));
        }

        /// <summary>
        /// Validates that the property is a valid Id for another entity.
        /// Allows validation of root entities references.
        /// </summary>
        /// <typeparam name="T">The base object validated.</typeparam>
        /// <typeparam name="TTargetEntity">The type of entity to check for by Id.</typeparam>
        /// <param name="ruleBuilder">The <see cref="IRuleBuilder{T, TProperty}"/>.</param>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider"/>.</param>
        /// <returns>The updated <see cref="IRuleBuilder{T, TProperty}"/>.</returns>
        public static IRuleBuilderOptions<T, string> ReferencesById<T, TTargetEntity>(
            this IRuleBuilder<T, string> ruleBuilder,
            IServiceProvider serviceProvider)
            where TTargetEntity : class, IIdentifiable
        {
            if (ruleBuilder is null)
            {
                throw new ArgumentNullException(nameof(ruleBuilder));
            }

            return ruleBuilder.SetValidator(
                new ReferencesByIdValidator<TTargetEntity>(
                    serviceProvider.GetRequiredService<IRepositoryGet<TTargetEntity>>()));
        }
    }
}
