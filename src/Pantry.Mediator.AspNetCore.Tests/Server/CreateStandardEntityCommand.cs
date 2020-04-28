using FluentValidation;
using Pantry.Mediator.Repositories.Commands;

namespace Pantry.Mediator.AspNetCore.Tests.Server
{
    public class CreateStandardEntityCommand : CreateCommand<StandardEntity>
    {
        public string? Name { get; set; }

        public class Validator : AbstractValidator<CreateStandardEntityCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();
            }
        }
    }
}
