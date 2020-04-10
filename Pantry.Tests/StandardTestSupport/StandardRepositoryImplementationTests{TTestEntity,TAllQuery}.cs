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

namespace Pantry.Tests.StandardTestSupport
{
    /// <summary>
    /// Base class for standard test suit that repository implementation can use & follow.
    /// </summary>
    /// <typeparam name="TTestEntity">The type of test entities to use.</typeparam>
    /// <typeparam name="TAllQuery">The type of <see cref="AllQuery{TTestEntity}"/> to use.</typeparam>
    public abstract class StandardRepositoryImplementationTests<TTestEntity, TAllQuery>
        where TTestEntity : class, IIdentifiable, IETaggable, new()
        where TAllQuery : AllQuery<TTestEntity>, new()
    {
        private readonly Lazy<IHost> _lazyHost;

        public StandardRepositoryImplementationTests(ITestOutputHelper outputHelper)
        {
            _lazyHost = new Lazy<IHost>(BuildHost);
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// Gets the <see cref="ITestOutputHelper"/>.
        /// </summary>
        protected ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// Gets the <see cref="IHost"/>.
        /// </summary>
        protected IHost Host => _lazyHost.Value;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected IServiceProvider ServiceProvider => Host.Services;

        /// <summary>
        /// Gets the test entity generator.
        /// </summary>
        protected abstract Faker<TTestEntity> TestEntityGenerator { get; }

        [Fact]
        public virtual async Task ItShouldCRUD()
        {
            var entities = TestEntityGenerator.Generate(3);
            var repository = ServiceProvider.GetRequiredService<ICrudRepository<TTestEntity>>();

            var addedEntities = new List<TTestEntity>();
            foreach (var entity in entities)
            {
                addedEntities.Add(await repository.AddAsync(entity));
            }

            var result = await repository.GetByIdAsync(addedEntities.First().Id);
            result.Id.Should().NotBeNullOrEmpty();
            result.ETag.Should().NotBeNullOrEmpty();
            result.Should().BeEquivalentTo(entities.First(), options => options.Excluding(x => x.ETag));

            var multipleResults = await repository.TryGetByIdsAsync(entities.Select(x => x.Id));
            multipleResults.Should().HaveSameCount(entities);

            var updatedEntities = new List<TTestEntity>();
            foreach (var entity in addedEntities)
            {
                var updatedEntity = TestEntityGenerator.Generate();
                updatedEntity.Id = entity.Id;
                updatedEntity.ETag = entity.ETag;

                var resultUpdate = await repository.UpdateAsync(updatedEntity);
                resultUpdate.Should().BeEquivalentTo(updatedEntity, options => options.Excluding(x => x.ETag));
                resultUpdate.ETag.Should().NotBe(entity.ETag);
                updatedEntities.Add(resultUpdate);
            }

            foreach (var entity in updatedEntities)
            {
                var deleteResult = await repository.TryDeleteAsync(entity);
                deleteResult.Should().BeTrue();
            }
        }

        [Fact]
        public virtual async Task ItShouldThrowOnAddConflicts()
        {
            var entity = TestEntityGenerator.Generate();
            var repository = ServiceProvider.GetRequiredService<IRepositoryAdd<TTestEntity>>();

            entity = await repository.AddAsync(entity);

            var secondEntitySameId = TestEntityGenerator
                .RuleFor(x => x.Id, entity.Id)
                .Generate();

            Func<Task> act = async () => await repository.AddAsync(secondEntitySameId);

            act.Should().Throw<ConflictException>();
        }

        [Fact]
        public virtual async Task ItShouldThrowWhenNotFound()
        {
            var entity = TestEntityGenerator.Generate();
            entity.Id = Guid.NewGuid().ToString();
            var repository = ServiceProvider.GetRequiredService<ICrudRepository<TTestEntity>>();
            Func<Task> act = async () => await repository.GetByIdAsync(entity.Id);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(entity.Id);

            act = async () => await repository.UpdateAsync(entity);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(entity.Id);
        }

        [Fact]
        public virtual async Task ItShouldQueryAll()
        {
            var repository = ServiceProvider.GetRequiredService<IRepository<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(10);
            foreach (var entity in entities)
            {
                await repository.AddAsync(entity);
            }

            var query = new TAllQuery
            {
                Limit = 3,
            };
            var result = await repository.FindAsync(query);
            result.Should().HaveCount(3);
            result.ContinuationToken.Should().NotBeNullOrEmpty();

            query = new TAllQuery
            {
                Limit = 3,
                ContinuationToken = result.ContinuationToken,
            };
            var secondResult = await repository.FindAsync(query);
            secondResult.Should().HaveCount(3);
            secondResult.ContinuationToken.Should().NotBeNullOrEmpty();

            secondResult.First().Id.Should().NotBe(result.First().Id);
        }

        /// <summary>
        /// Builds the test <see cref="IHost"/>.
        /// </summary>
        /// <returns>The <see cref="IHost"/>.</returns>
        protected virtual IHost BuildHost()
        {
            var hostBuilder = Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config
                        .AddInMemoryCollection(AdditionalConfigurationValues())
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging(logging =>
                {
                    logging
                        .AddXUnit(OutputHelper)
                        .AddFilter(_ => true);
                })
                .ConfigureServices((context, services) =>
                {
                    RegisterTestServices<TTestEntity>(context, services);
                });

            PostConfigureHostBuilder(hostBuilder);

            return hostBuilder.Build();
        }

        /// <summary>
        /// Registers the services under test.
        /// </summary>
        /// <typeparam name="TEntity">The test entity type.</typeparam>
        /// <param name="context">The <see cref="HostBuilderContext"/>.</param>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        protected abstract void RegisterTestServices<TEntity>(HostBuilderContext context, IServiceCollection services)
            where TEntity : class, IIdentifiable, IETaggable, new();

        /// <summary>
        /// Additional values to add to the <see cref="IConfiguration"/>.
        /// Those can be overriden by environment variables after.
        /// </summary>
        /// <returns>The list of configuration variables.</returns>
        protected virtual IEnumerable<KeyValuePair<string, string>> AdditionalConfigurationValues() => Enumerable.Empty<KeyValuePair<string, string>>();

        /// <summary>
        /// Any additional configuration to apply to the <see cref="IHostBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHostBuilder"/>.</param>
        protected virtual void PostConfigureHostBuilder(IHostBuilder builder)
        {
            // No-op.
        }
    }
}
