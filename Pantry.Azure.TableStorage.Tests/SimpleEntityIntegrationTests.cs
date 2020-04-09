using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pantry.Exceptions;
using Pantry.Queries;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Azure.TableStorage.Tests
{
    public class SimpleEntityIntegrationTests
    {
        private const string StorageConnectionString = nameof(StorageConnectionString);

        private readonly IHost _host;

        public SimpleEntityIntegrationTests(ITestOutputHelper outputHelper)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config
                        .AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { $"ConnectionStrings:{StorageConnectionString}", "UseDevelopmentStorage=true" },
                        })
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging(logging =>
                {
                    logging
                        .AddXUnit(outputHelper)
                        .AddFilter(_ => true);
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddAzureTableStorageRepository<SimpleEntity>()
                        .WithConnectionStringNamed(StorageConnectionString);
                })
                .Build();
        }

        private Faker<SimpleEntity> SimpleEntityGenerator => new Faker<SimpleEntity>()
            .RuleFor(x => x.Name, f => f.Name.JobTitle());

        private IServiceProvider ServiceProvider => _host.Services;

        [Fact]
        public async Task ItShouldCRUD()
        {
            var entities = SimpleEntityGenerator.Generate(3);
            var repository = ServiceProvider.GetRequiredService<ICrudRepository<SimpleEntity>>();

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
                    new SimpleEntity
                    {
                        Id = entity.Id,
                        Name = SimpleEntityGenerator.Generate().Name,
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
            var entity = SimpleEntityGenerator.Generate();
            var repository = ServiceProvider.GetRequiredService<ICanAdd<SimpleEntity>>();

            entity = await repository.AddAsync(entity);

            var secondEntitySameId = SimpleEntityGenerator
                .RuleFor(x => x.Id, entity.Id)
                .Generate();

            Func<Task> act = async () => await repository.AddAsync(secondEntitySameId);

            act.Should().Throw<ConflictException>();
        }

        [Fact]
        public async Task ItShouldThrowWhenNotFound()
        {
            var entity = SimpleEntityGenerator.Generate();
            entity.Id = Guid.NewGuid().ToString();
            var repository = ServiceProvider.GetRequiredService<ICrudRepository<SimpleEntity>>();
            Func<Task> act = async () => await repository.GetByIdAsync(entity.Id);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(entity.Id);

            act = async () => await repository.UpdateAsync(entity);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(entity.Id);
        }

        [Fact]
        public async Task ItShouldQueryAll()
        {
            var repository = ServiceProvider.GetRequiredService<IRepository<SimpleEntity>>();
            var entities = SimpleEntityGenerator.Generate(10);
            foreach (var entity in entities)
            {
                await repository.AddAsync(entity);
            }

            var query = new AllQuery<SimpleEntity>
            {
                Limit = 3,
            };
            var result = await repository.FindAsync(query);
            result.Should().HaveCount(3);
            result.ContinuationToken.Should().NotBeNullOrEmpty();

            query = new AllQuery<SimpleEntity>
            {
                Limit = 3,
                ContinuationToken = result.ContinuationToken,
            };
            var secondResult = await repository.FindAsync(query);
            secondResult.Should().HaveCount(3);
            secondResult.ContinuationToken.Should().NotBeNullOrEmpty();

            secondResult.First().Id.Should().NotBe(result.First().Id);

            var withBaseClassSupport = await repository.FindAsync(new QueryAllSimpleEntities
            {
                Limit = 5,
            });
            withBaseClassSupport.Should().HaveCount(5);
        }

        [Fact]
        public async Task ItShouldQueryMirror()
        {
            var repository = ServiceProvider.GetRequiredService<IRepository<SimpleEntity>>();
            var entities = SimpleEntityGenerator.Generate(10);
            foreach (var entity in entities)
            {
                await repository.AddAsync(entity);
            }

            var query = new MirrorQuery<SimpleEntity>
            {
                Limit = 3,
            };
            var result = await repository.FindAsync(query);
            result.Should().HaveCount(3);
            result.ContinuationToken.Should().NotBeNullOrEmpty();

            query.Mirror.Name = entities.First().Name;
            result = await repository.FindAsync(query);
            result.Count().Should().BeGreaterOrEqualTo(1);

            query.Mirror.Name = Guid.NewGuid().ToString();
            result = await repository.FindAsync(query);
            result.Should().HaveCount(0);
        }

        private class QueryAllSimpleEntities : AllQuery<SimpleEntity>
        {
        }
    }
}
