using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pantry.Mediator.Tests;
using Xunit;

namespace Pantry.Mediator.FluentValidation.Tests
{
    public class FluentValidationDomainRequestMiddlewareTests
    {
        [Fact]
        public async Task ItShouldValidateRequests()
        {
            var handler = new Mock<IDomainRequestHandler<SampleCommand, SampleEntity>>();
            var services = new ServiceCollection()
                .AddMediator()
                .AddFluentValidationRequestMiddleware()
                .AddTransient(sp => handler.Object)
                .AddValidatorsFromAssemblyContaining<FluentValidationDomainRequestMiddlewareTests>()
                .BuildServiceProvider();

            var command = new SampleCommand();
            var mediator = services.GetRequiredService<IMediator>();

            Func<Task> act = async () => await mediator.ExecuteAsync(command);
            act.Should().Throw<ValidationException>().WithMessage("*Name*");
        }

        [Fact]
        public async Task ItShouldWorkWhenNoValidators()
        {
            var handler = new Mock<IDomainRequestHandler<SampleCommand, SampleEntity>>();
            var services = new ServiceCollection()
                .AddMediator()
                .AddFluentValidationRequestMiddleware()
                .AddTransient(sp => handler.Object)
                .BuildServiceProvider();

            var command = new SampleCommand();
            var mediator = services.GetRequiredService<IMediator>();

            Func<Task> act = async () => await mediator.ExecuteAsync(command);
            act.Should().NotThrow();
        }
    }
}
