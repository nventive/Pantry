using FluentValidation;
using Pantry.Mediator.Tests;

namespace Pantry.Mediator.FluentValidation.Tests
{
    public class SampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SampleCommandValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
        }
    }
}
