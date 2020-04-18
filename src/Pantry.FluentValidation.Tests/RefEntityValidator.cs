using System;
using FluentValidation;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.FluentValidation.Tests
{
    public class RefEntityValidator : AbstractValidator<RefEntity>
    {
        public RefEntityValidator(IServiceProvider serviceProvider)
        {
            RuleFor(x => x.StandardEntityId).ReferencesById<RefEntity, StandardEntity>(serviceProvider);
        }
    }
}
