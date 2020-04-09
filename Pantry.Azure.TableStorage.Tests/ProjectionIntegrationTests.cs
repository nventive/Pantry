using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pantry.Mapping;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Azure.TableStorage.Tests
{
    public class ProjectionIntegrationTests
    {
        private const string StorageConnectionString = nameof(StorageConnectionString);

        private readonly IHost _host;

        public ProjectionIntegrationTests(ITestOutputHelper outputHelper)
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
                        .WithTableEntityMapper<SimpleEntity, SimpleEntityCustomMapper>()
                        .WithConnectionStringNamed(StorageConnectionString, nameof(SimpleEntityCustomMapper));
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

        private class SimpleEntityCustomMapper : IMapper<SimpleEntity, DynamicTableEntity>
        {
            public DynamicTableEntity MapToDestination(SimpleEntity source)
            {
                var dynamicTableEntity = new DynamicTableEntity(source.Id, source.Id);
                dynamicTableEntity["OtherName"] = EntityProperty.GeneratePropertyForString(source.Name);
                return dynamicTableEntity;
            }

            public SimpleEntity MapToSource(DynamicTableEntity destination)
            {
                return new SimpleEntity
                {
                    Id = destination.RowKey,
                    Name = destination["OtherName"].StringValue,
                };
            }
        }
    }
}
