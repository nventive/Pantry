using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Pantry.Exceptions;
using Pantry.Generators;
using Pantry.Tests.StandardTestSupport;
using Xunit;

namespace Pantry.Tests
{
    public class CrudRepositoryExtensionsTests
    {
        [Fact]
        public async Task ItShouldApplyUpdate()
        {
            var currentEntity = new StandardEntity
            {
                Id = await new GuidIdGenerator<StandardEntity>().Generate(null),
            };

            var updatedEntity = new StandardEntity
            {
                Id = currentEntity.Id,
            };

            var updatedEntityThroughRepo = new StandardEntity
            {
                Id = currentEntity.Id,
            };

            var repo = new Mock<ICrudRepository<StandardEntity>>();
            repo.Setup(x => x.GetByIdAsync(currentEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentEntity);

            repo.Setup(x => x.UpdateAsync(updatedEntity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedEntityThroughRepo);

            var result = await repo.Object.UpdateLatestAsync(
                currentEntity.Id,
                e =>
                {
                    e.Should().Be(currentEntity);
                    return updatedEntity;
                });

            result.Should().Be(updatedEntityThroughRepo);
        }

        [Fact]
        public async Task ItShouldApplyUpdatesRepeateadlyWhenConcurrencyIssue()
        {
            var currentEntity = new StandardEntity
            {
                Id = await new GuidIdGenerator<StandardEntity>().Generate(null),
            };

            var updatedEntity = new StandardEntity
            {
                Id = currentEntity.Id,
            };

            var updatedEntityThroughRepo = new StandardEntity
            {
                Id = currentEntity.Id,
            };

            var repo = new Mock<ICrudRepository<StandardEntity>>();
            repo.Setup(x => x.GetByIdAsync(currentEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentEntity);

            repo.SetupSequence(x => x.UpdateAsync(updatedEntity, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConcurrencyException())
                .ThrowsAsync(new ConcurrencyException())
                .ReturnsAsync(updatedEntityThroughRepo);

            var result = await repo.Object.UpdateLatestAsync(
                currentEntity.Id,
                e =>
                {
                    e.Should().Be(currentEntity);
                    return updatedEntity;
                });

            result.Should().Be(updatedEntityThroughRepo);

            repo.Verify(x => x.GetByIdAsync(currentEntity.Id, It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Fact]
        public async Task ItShouldGiveUpWhenMaximumTimesAchieved()
        {
            var currentEntity = new StandardEntity
            {
                Id = await new GuidIdGenerator<StandardEntity>().Generate(null),
            };

            var updatedEntity = new StandardEntity
            {
                Id = currentEntity.Id,
            };

            var updatedEntityThroughRepo = new StandardEntity
            {
                Id = currentEntity.Id,
            };

            var repo = new Mock<ICrudRepository<StandardEntity>>();
            repo.Setup(x => x.GetByIdAsync(currentEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentEntity);

            repo.Setup(x => x.UpdateAsync(updatedEntity, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConcurrencyException());

            Func<Task> act = async () => await repo.Object.UpdateLatestAsync(
                currentEntity.Id,
                e =>
                {
                    e.Should().Be(currentEntity);
                    return updatedEntity;
                },
                10);

            act.Should().Throw<ConcurrencyException>();

            repo.Verify(x => x.GetByIdAsync(currentEntity.Id, It.IsAny<CancellationToken>()), Times.Exactly(11));
        }
    }
}
