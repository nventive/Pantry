using FluentValidation;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.FluentValidation.Tests
{
    public class StandardEntityValidator : AbstractValidator<StandardEntity>
    {
        public StandardEntityValidator()
        {
            RuleSet(RuleSets.Add, () =>
            {
                RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
            });
            RuleSet(RuleSets.Update, () =>
            {
                RuleFor(x => x.Id).NotNull().NotEmpty();
                RuleFor(x => x.Age).GreaterThanOrEqualTo(21);
            });

            RuleFor(x => x.Age).LessThan(150);
        }
    }
}
