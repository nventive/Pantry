using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pantry.Exceptions;
using Pantry.Queries;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.InMemory.Tests
{
    public class ConcurrentDictionaryTests
    {
        private readonly IHost _host;

        public ConcurrentDictionaryTests(ITestOutputHelper outputHelper)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging
                        .AddXUnit(outputHelper)
                        .AddFilter(_ => true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddConcurrentDictionaryRepository<TestEntity>();
                })
                .Build();
        }

        private Faker<TestEntity> TestEntityGenerator => new Faker<TestEntity>()
            .RuleFor(x => x.Name, f => f.Name.JobTitle());

        private IServiceProvider ServiceProvider => _host.Services;

        [Fact]
        public async Task ItShouldCRUD()
        {
            var entities = TestEntityGenerator.Generate(3);
            var repository = ServiceProvider.GetRequiredService<ICrudRepository<TestEntity>>();

            foreach (var entity in entities)
            {
                await repository.AddAsync(entity);
            }

            var result = await repository.GetByIdAsync(entities.First().Id);
            result.Id.Should().NotBeNullOrEmpty();
            result.Name.Should().Be(entities.First().Name);

            var multipleResults = await repository.TryGetByIdsAsync(entities.Select(x => x.Id));
            multipleResults.Should().HaveSameCount(entities);

            foreach (var entity in entities)
            {
                var resultUpdate = await repository.UpdateAsync(
                    new TestEntity
                    {
                        Id = entity.Id,
                        Name = TestEntityGenerator.Generate().Name,
                    });

                resultUpdate.Name.Should().NotBe(entity.Name);
            }

            foreach (var entity in entities)
            {
                var deleteResult = await repository.TryDeleteAsync(entity);
                deleteResult.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ItShouldThrowOnAddConflicts()
        {
            var entity = TestEntityGenerator.Generate();
            var repository = ServiceProvider.GetRequiredService<IRepositoryAdd<TestEntity>>();

            entity = await repository.AddAsync(entity);

            var secondEntitySameId = TestEntityGenerator
                .RuleFor(x => x.Id, entity.Id)
                .Generate();

            Func<Task> act = async () => await repository.AddAsync(secondEntitySameId);

            act.Should().Throw<ConflictException>();
        }

        [Fact]
        public async Task ItShouldThrowWhenNotFound()
        {
            var entity = TestEntityGenerator.Generate();
            entity.Id = Guid.NewGuid().ToString();
            var repository = ServiceProvider.GetRequiredService<ICrudRepository<TestEntity>>();
            Func<Task> act = async () => await repository.GetByIdAsync(entity.Id);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(entity.Id);

            act = async () => await repository.UpdateAsync(entity);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(entity.Id);
        }

        [Fact]
        public async Task ItShouldQueryAll()
        {
            var repository = ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var entities = TestEntityGenerator.Generate(10);
            foreach (var entity in entities)
            {
                await repository.AddAsync(entity);
            }

            var query = new QueryAllTestEntities
            {
                Limit = 3,
            };
            var result = await repository.FindAsync(query);
            result.Should().HaveCount(3);
            result.ContinuationToken.Should().NotBeNullOrEmpty();

            query = new QueryAllTestEntities
            {
                Limit = 3,
                ContinuationToken = result.ContinuationToken,
            };
            var secondResult = await repository.FindAsync(query);
            secondResult.Should().HaveCount(3);
            secondResult.ContinuationToken.Should().NotBeNullOrEmpty();

            secondResult.First().Id.Should().NotBe(result.First().Id);
        }

        [Fact]
        public async Task ItShouldQueryMirror()
        {
            var repository = ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var entities = TestEntityGenerator.Generate(10);
            foreach (var entity in entities)
            {
                await repository.AddAsync(entity);
            }

            var query = new QueryTestEntities
            {
                Limit = 3,
            };
            var result = await repository.FindAsync(query);
            result.Should().HaveCount(3);
            result.ContinuationToken.Should().NotBeNullOrEmpty();

            query.NameEq = entities.First().Name;
            result = await repository.FindAsync(query);
            result.Count().Should().BeGreaterOrEqualTo(1);

            query.NameEq = Guid.NewGuid().ToString();
            result = await repository.FindAsync(query);
            result.Should().HaveCount(0);
        }

        private class QueryAllTestEntities : AllQuery<TestEntity>
        {
        }

        private class QueryTestEntities : MirrorQuery<TestEntity>
        {
            public string? NameEq
            {
                get => Mirror.Name;
                set { (Mirror ??= new TestEntity()).Name = value; }
            }
        }
    }
}
