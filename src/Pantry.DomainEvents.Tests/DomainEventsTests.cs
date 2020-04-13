using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pantry.Tests.StandardTestSupport;
using Pantry.Traits;
using Xunit;

namespace Pantry.DomainEvents.Tests
{
    public class DomainEventsTests
    {
        [Fact]
        public async Task ItShouldWorkWithNoHandlers()
        {
            var services = GetServiceCollectionWithDomainEvents();
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ICrudRepository<StandardEntity>>();

            var result = await repo.AddAsync(new StandardEntity());
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ItShouldEmitAddedEvents()
        {
            var addedHandler1 = new Mock<IDomainEventHandler<EntityAddedDomainEvent<StandardEntity>>>();
            var addedHandler2 = new Mock<IDomainEventHandler<EntityAddedDomainEvent<StandardEntity>>>();
            var updatedHandler = new Mock<IDomainEventHandler<EntityUpdatedDomainEvent<StandardEntity>>>();
            var services = GetServiceCollectionWithDomainEvents()
                .AddSingleton(addedHandler1.Object)
                .AddSingleton(addedHandler2.Object)
                .AddSingleton(updatedHandler.Object);
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ICrudRepository<StandardEntity>>();

            var result = await repo.AddAsync(new StandardEntity());
            result.Should().NotBeNull();

            addedHandler1.Verify(x => x.HandleAsync(It.Is<EntityAddedDomainEvent<StandardEntity>>(evt => evt.Entity == result)));
            addedHandler2.Verify(x => x.HandleAsync(It.Is<EntityAddedDomainEvent<StandardEntity>>(evt => evt.Entity == result)));
            updatedHandler.Verify(x => x.HandleAsync(It.IsAny<EntityUpdatedDomainEvent<StandardEntity>>()), Times.Never);
        }

        [Fact]
        public async Task ItShouldEmitUpdatedEvents()
        {
            var updatedHandler1 = new Mock<IDomainEventHandler<EntityUpdatedDomainEvent<StandardEntity>>>();
            var updatedHandler2 = new Mock<IDomainEventHandler<EntityUpdatedDomainEvent<StandardEntity>>>();
            var removedHandler = new Mock<IDomainEventHandler<EntityRemovedDomainEvent<StandardEntity>>>();
            var services = GetServiceCollectionWithDomainEvents()
                .AddSingleton(updatedHandler1.Object)
                .AddSingleton(updatedHandler2.Object)
                .AddSingleton(removedHandler.Object);
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ICrudRepository<StandardEntity>>();

            var result = await repo.AddAsync(new StandardEntity());
            result = await repo.UpdateAsync(result);
            result.Should().NotBeNull();

            updatedHandler1.Verify(x => x.HandleAsync(It.Is<EntityUpdatedDomainEvent<StandardEntity>>(evt => evt.Entity == result)));
            updatedHandler1.Verify(x => x.HandleAsync(It.Is<EntityUpdatedDomainEvent<StandardEntity>>(evt => evt.Entity == result)));
            removedHandler.Verify(x => x.HandleAsync(It.IsAny<EntityRemovedDomainEvent<StandardEntity>>()), Times.Never);
        }

        [Fact]
        public async Task ItShouldEmitAddOrUpdatedEvents()
        {
            var updatedHandler = new Mock<IDomainEventHandler<EntityUpdatedDomainEvent<StandardEntity>>>();
            var addedHandler = new Mock<IDomainEventHandler<EntityAddedDomainEvent<StandardEntity>>>();
            var services = GetServiceCollectionWithDomainEvents()
                .AddSingleton(updatedHandler.Object)
                .AddSingleton(addedHandler.Object);
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ICrudRepository<StandardEntity>>();

            var existingEntity = new StandardEntity();
            existingEntity = await repo.AddAsync(existingEntity);
            var (existingEntityResult, existingEntityAdded) = await repo.AddOrUpdateAsync(existingEntity);
            var newEntity = new StandardEntity();
            var (newEntityResult, newEntityAdded) = await repo.AddOrUpdateAsync(newEntity);

            addedHandler.Verify(x => x.HandleAsync(It.Is<EntityAddedDomainEvent<StandardEntity>>(evt => evt.Entity == existingEntity)));
            addedHandler.Verify(x => x.HandleAsync(It.Is<EntityAddedDomainEvent<StandardEntity>>(evt => evt.Entity == newEntityResult)));
            updatedHandler.Verify(x => x.HandleAsync(It.Is<EntityUpdatedDomainEvent<StandardEntity>>(evt => evt.Entity == existingEntityResult)));
        }

        [Fact]
        public async Task ItShouldEmitRemovedEventsWhenFound()
        {
            var removedHandler = new Mock<IDomainEventHandler<EntityRemovedDomainEvent<StandardEntity>>>();
            var services = GetServiceCollectionWithDomainEvents()
                .AddSingleton(removedHandler.Object);
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ICrudRepository<StandardEntity>>();

            var result = await repo.AddAsync(new StandardEntity());
            await repo.RemoveAsync(result);

            removedHandler.Verify(x => x.HandleAsync(It.Is<EntityRemovedDomainEvent<StandardEntity>>(evt => evt.EntityId == result.Id)));
        }

        [Fact]
        public async Task ItShouldNotEmitRemovedEventsWhenNotFound()
        {
            var removedHandler = new Mock<IDomainEventHandler<EntityRemovedDomainEvent<StandardEntity>>>();
            var services = GetServiceCollectionWithDomainEvents()
                .AddSingleton(removedHandler.Object);
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<IRepositoryRemove<StandardEntity>>();

            await repo.TryRemoveAsync(Guid.NewGuid().ToString());

            removedHandler.Verify(x => x.HandleAsync(It.IsAny<EntityRemovedDomainEvent<StandardEntity>>()), Times.Never);
        }

        private IServiceCollection GetServiceCollectionWithDomainEvents()
        {
            var svc = new ServiceCollection();
            svc
                .AddConcurrentDictionaryRepository<StandardEntity>()
                .EmitDomainEvents();

            return svc;
        }
    }
}
