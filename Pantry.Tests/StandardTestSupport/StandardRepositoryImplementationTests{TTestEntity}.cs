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
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Tests.StandardTestSupport
{
    /// <summary>
    /// Base class for standard test suit that repository implementation can use & follow.
    /// </summary>
    /// <typeparam name="TTestEntity">The type of test entities to use.</typeparam>
    public abstract class StandardRepositoryImplementationTests<TTestEntity>
        where TTestEntity : class, IIdentifiable, IETaggable, new()
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
        /// Gets a <see cref="IRepo{TTestEntity}"/>.
        /// </summary>
        protected IRepository<TTestEntity> Repo => ServiceProvider.GetRequiredService<IRepository<TTestEntity>>();

        /// <summary>
        /// Gets a <see cref="IIdGenerator{TTestEntity}"/>.
        /// </summary>
        protected IIdGenerator<TTestEntity> IdGenerator => ServiceProvider.GetRequiredService<IIdGenerator<TTestEntity>>();

        /// <summary>
        /// Gets the test entity generator.
        /// </summary>
        protected abstract Faker<TTestEntity> TestEntityGenerator { get; }

        [Fact]
        public virtual async Task ItShouldAdd()
        {
            var entity = TestEntityGenerator.Generate();

            var result = await Repo.AddAsync(entity);

            result.Id.Should().NotBeNullOrEmpty();
            result.ETag.Should().NotBeNullOrEmpty();
            result.Should().BeEquivalentTo(entity, opt => opt.Excluding(x => x.Id).Excluding(x => x.ETag));
        }

        [Fact]
        public virtual async Task ItShouldNotAddWhenNull()
        {
            Func<Task> act = async () => await Repo.AddAsync(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public virtual async Task ItShouldAddWithExistingId()
        {
            var existingId = await IdGenerator.Generate(null);
            var entity = TestEntityGenerator
                .RuleFor(x => x.Id, existingId)
                .Generate();

            Func<Task> act = async () => await Repo.AddAsync(null!);

            var result = await Repo.AddAsync(entity);

            result.Id.Should().Be(existingId);
        }

        [Fact]
        public virtual async Task ItShouldNotAddWhenConflict()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);
            var newEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .Generate();

            Func<Task> act = async () => await Repo.AddAsync(newEntity);

            act.Should().Throw<ConflictException>().Which.TargetId.Should().Be(newEntity.Id);

            var result = await Repo.GetByIdAsync(existingEntity.Id);
            result.Should().BeEquivalentTo(existingEntity, opt => opt.Excluding(x => x.ETag));
        }

        [Fact]
        public virtual async Task ItShouldTryGetById()
        {
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);
            var referenceEntity = entities[1];

            var result = await Repo.TryGetByIdAsync(referenceEntity.Id);

            result.Should().BeEquivalentTo(referenceEntity, opt => opt.Excluding(x => x.ETag));
        }

        [Fact]
        public virtual async Task ItShouldTryGetByIdWhenNull()
        {
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);
            var notExistingId = await IdGenerator.Generate();

            var result = await Repo.TryGetByIdAsync(notExistingId);

            result.Should().BeNull();
        }

        [Fact]
        public virtual async Task ItShouldGetById()
        {
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);
            var referenceEntity = entities[1];

            var result = await Repo.GetByIdAsync(referenceEntity.Id);

            result.Should().BeEquivalentTo(referenceEntity, opt => opt.Excluding(x => x.ETag));
        }

        [Fact]
        public virtual async Task ItShouldGetByIdWhenNotFound()
        {
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);
            var notExistingId = await IdGenerator.Generate();

            Func<Task> act = async () => await Repo.GetByIdAsync(notExistingId);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(notExistingId);
        }

        [Fact]
        public virtual async Task ItShouldTryGetByIds()
        {
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);

            var result = await Repo.TryGetByIdsAsync(entities.Select(x => x.Id));

            result.Should().HaveSameCount(entities);
            result.Keys.Should().Contain(entities.Select(x => x.Id));
            result[entities.First().Id].Should().BeEquivalentTo(entities.First(), opt => opt.Excluding(x => x.ETag));
        }

        [Fact]
        public virtual async Task ItShouldTryGetByIdsWhenSomeNotFound()
        {
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);
            var notExistingId = await IdGenerator.Generate();

            var result = await Repo.TryGetByIdsAsync(entities.Select(x => x.Id).Union(new[] { notExistingId }));

            result.Should().HaveSameCount(entities);
            result.Keys.Should().Contain(entities.Select(x => x.Id));
            result.Keys.Should().NotContain(notExistingId);
            result.Keys.Should().NotContainNulls();
            result[entities.First().Id].Should().BeEquivalentTo(entities.First(), opt => opt.Excluding(x => x.ETag));
        }

        [Fact]
        public virtual async Task ItShouldExists()
        {
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);
            var referenceEntity = entities[1];

            var result = await Repo.ExistsAsync(referenceEntity.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public virtual async Task ItShouldNotExistsWhenNotFound()
        {
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);
            var notExistingId = await IdGenerator.Generate();

            var result = await Repo.ExistsAsync(notExistingId);

            result.Should().BeFalse();
        }

        [Fact]
        public virtual async Task ItShouldUpdateUnconditionally()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .Generate();

            var result = await Repo.UpdateAsync(updatedEntity);

            result.ETag.Should().NotBeNullOrEmpty();
            result.Should().BeEquivalentTo(updatedEntity, opt => opt.Excluding(x => x.ETag));
            result.Should().NotBeEquivalentTo(existingEntity, opt => opt.Excluding(x => x.ETag));
        }

        [Fact]
        public virtual async Task ItShouldUpdateConditionally()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .RuleFor(x => x.ETag, existingEntity.ETag)
                .Generate();

            var result = await Repo.UpdateAsync(updatedEntity);

            result.ETag.Should().NotBeNullOrEmpty();
            result.ETag.Should().NotBe(existingEntity.ETag);
            result.Should().BeEquivalentTo(updatedEntity, opt => opt.Excluding(x => x.ETag));
            result.Should().NotBeEquivalentTo(existingEntity, opt => opt.Excluding(x => x.ETag));
        }

        [Fact]
        public virtual async Task ItShouldNotUpdateIfNotFound()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);
            var notExistingId = await IdGenerator.Generate();
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, notExistingId)
                .Generate();

            Func<Task> act = async () => await Repo.UpdateAsync(updatedEntity);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(notExistingId);
        }

        [Fact]
        public virtual async Task ItShouldNotUpdateConditionallyWhenConcurrencyIssue()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .RuleFor(x => x.ETag, await IdGenerator.Generate())
                .Generate();

            Func<Task> act = async () => await Repo.UpdateAsync(updatedEntity);

            act.Should().Throw<ConcurrencyException>().Which.TargetId.Should().Be(existingEntity.Id);
        }

        [Fact]
        public virtual async Task ItShouldRemoveById()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);

            await Repo.RemoveAsync(existingEntity.Id);

            (await Repo.TryGetByIdAsync(existingEntity.Id)).Should().BeNull();
        }

        [Fact]
        public virtual async Task ItShouldNotRemoveByIdWhenNotFound()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);
            var notExistingId = await IdGenerator.Generate();

            Func<Task> act = async () => await Repo.RemoveAsync(notExistingId);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(notExistingId);
        }

        [Fact]
        public virtual async Task ItShouldRemoveByEntity()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);

            await Repo.RemoveAsync(existingEntity);

            (await Repo.TryGetByIdAsync(existingEntity.Id)).Should().BeNull();
        }

        [Fact]
        public virtual async Task ItShouldNotRemoveByEntityWhenNotFound()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);
            var notExistingEntity = TestEntityGenerator
                .RuleFor(x => x.Id, await IdGenerator.Generate())
                .Generate();

            Func<Task> act = async () => await Repo.RemoveAsync(notExistingEntity);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(notExistingEntity.Id);
        }

        [Fact]
        public virtual async Task ItShouldTryRemoveById()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);

            var result = await Repo.TryRemoveAsync(existingEntity.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public virtual async Task ItShouldTryRemoveByIdWhenNotFound()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);
            var notExistingId = await IdGenerator.Generate();

            var result = await Repo.TryRemoveAsync(notExistingId);

            result.Should().BeFalse();
        }

        [Fact]
        public virtual async Task ItShouldTryRemoveByEntity()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);

            var result = await Repo.TryRemoveAsync(existingEntity);

            result.Should().BeTrue();
        }

        [Fact]
        public virtual async Task ItShouldTryRemoveByEntityWhenNotFound()
        {
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, existingEntity);
            var notExistingEntity = TestEntityGenerator
                .RuleFor(x => x.Id, await IdGenerator.Generate())
                .Generate();

            var result = await Repo.TryRemoveAsync(notExistingEntity);

            result.Should().BeFalse();
        }

        [Fact]
        public virtual async Task ItShouldFindAll()
        {
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);

            var result = await Repo.FindAllAsync(null);

            result.Count().Should().BeGreaterOrEqualTo(entities.Count);
        }

        [Fact]
        public virtual async Task ItShouldFindAllByPage()
        {
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);

            var firstResult = await Repo.FindAllAsync(null, limit: 3);

            firstResult.Count().Should().Be(3);
            firstResult.ContinuationToken.Should().NotBeNullOrEmpty();

            var secondResult = await Repo.FindAllAsync(firstResult.ContinuationToken, limit: 3);

            secondResult.Count().Should().BeLessOrEqualTo(3);
            secondResult.ContinuationToken.Should().NotBe(firstResult.ContinuationToken);
        }

        [Fact]
        public virtual async Task ItShouldFindAllWithZero()
        {
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);

            var result = await Repo.FindAllAsync(null, 0);

            result.Should().BeEmpty();
            result.ContinuationToken.Should().BeNullOrEmpty();
        }

        [Fact]
        public virtual async Task ItShouldFindAllWithNegativeNumber()
        {
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(Repo, entities);

            var result = await Repo.FindAllAsync(null, -3);

            result.Should().BeEmpty();
            result.ContinuationToken.Should().BeNullOrEmpty();
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
