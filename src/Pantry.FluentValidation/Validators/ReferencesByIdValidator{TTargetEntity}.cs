using System;
using FluentValidation.Validators;
using Pantry.Traits;

namespace Pantry.FluentValidation.Validators
{
    /// <summary>
    /// <see cref="PropertyValidator"/> that validates that a target entity exists.
    /// </summary>
    /// <typeparam name="TTargetEntity">The type of entity to check for by Id.</typeparam>
    public class ReferencesByIdValidator<TTargetEntity> : PropertyValidator
        where TTargetEntity : class, IIdentifiable
    {
        private readonly IRepositoryGet<TTargetEntity> _repoGetTargetEntity;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencesByIdValidator{TTargetEntity}"/> class.
        /// </summary>
        /// <param name="repoGetTargetEntity">The target entity repository.</param>
        public ReferencesByIdValidator(
            IRepositoryGet<TTargetEntity> repoGetTargetEntity)
            : base("'{PropertyValue}' is not a valid id for {EntityType}.")
        {
            _repoGetTargetEntity = repoGetTargetEntity ?? throw new ArgumentNullException(nameof(repoGetTargetEntity));
        }

        /// <inheritdoc/>
        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var targetId = context.PropertyValue as string;
            if (!string.IsNullOrEmpty(targetId))
            {
                if (!_repoGetTargetEntity.ExistsAsync(targetId).ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    context.MessageFormatter.AppendArgument("EntityType", typeof(TTargetEntity).Name);
                    return false;
                }
            }

            return true;
        }
    }
}
