﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pantry.Continuation;
using Pantry.Mediator.Exceptions;
using Xunit;

namespace Pantry.Mediator.Tests
{
    public class ServiceProviderMediatorTests
    {
        [Fact]
        public async Task ItShouldExecuteQueryHandlers()
        {
            var services = new ServiceCollection()
                .AddMediator()
                .AddTransient<IDomainRequestHandler<SampleQuery, IContinuationEnumerable<SampleEntity>>, SampleQuery.Handler>()
                .BuildServiceProvider();

            var request = new SampleQuery();
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(request);

            result.First().Id.Should().Be(typeof(SampleQuery.Handler).Name);
        }

        [Fact]
        public async Task ItShouldExecuteCommandHandlers()
        {
            var services = new ServiceCollection()
                .AddMediator()
                .AddTransient<IDomainRequestHandler<SampleCommand, SampleEntity>, SampleCommand.Handler>()
                .BuildServiceProvider();

            var request = new SampleCommand();
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(request);

            result.Id.Should().Be(typeof(SampleCommand.Handler).Name);
        }

        [Fact]
        public async Task ItShouldThrowWhenHandlerNotFound()
        {
            var services = new ServiceCollection()
                .AddMediator()
                .BuildServiceProvider();

            var request = new SampleCommand();
            var mediator = services.GetRequiredService<IMediator>();

            Func<Task> act = async () => await mediator.ExecuteAsync(request);

            act.Should().Throw<MediatorException>().WithMessage($"*{nameof(SampleCommand)}*");
        }

        [Fact]
        public async Task ItShouldPublishWithNoHandlers()
        {
            var services = new ServiceCollection()
                .AddMediator()
                .BuildServiceProvider();

            var domainEvent = new SampleEvent();
            var mediator = services.GetRequiredService<IMediator>();

            Func<Task> act = async () => await mediator.PublishAsync(domainEvent);

            act.Should().NotThrow();
        }

        [Fact]
        public async Task ItShouldPublishToHandlers()
        {
            var handler1 = new Mock<IDomainEventHandler<SampleEvent>>();
            var handler2 = new Mock<IDomainEventHandler<SampleEvent>>();

            var services = new ServiceCollection()
                .AddMediator()
                .AddSingleton(handler1.Object)
                .AddSingleton(handler2.Object)
                .BuildServiceProvider();

            var domainEvent = new SampleEvent();
            var mediator = services.GetRequiredService<IMediator>();

            await mediator.PublishAsync(domainEvent);

            handler1.Verify(x => x.HandleAsync(domainEvent));
            handler2.Verify(x => x.HandleAsync(domainEvent));
        }
    }
}
