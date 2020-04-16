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
using Pantry.Generators;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Tests.StandardTestSupport
{
    /// <summary>
    /// Base class for standard test suit that repository implementation can use & follow.
    /// </summary>
    /// <typeparam name="TRepository">The type of repository to test.</typeparam>
    /// <typeparam name="TTestEntity">The type of test entities to use.</typeparam>
    public abstract class StandardRepositoryImplementationTests<TRepository, TTestEntity>
        where TTestEntity : class, IIdentifiable, IETaggable, ITimestamped, new()
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
        /// Gets the Trepository.
        /// </summary>
        protected TRepository Repository => ServiceProvider.GetRequiredService<TRepository>();

        /// <summary>
        /// Gets a <see cref="IIdGenerator{TTestEntity}"/>.
        /// </summary>
        protected IIdGenerator<TTestEntity> IdGenerator => ServiceProvider.GetRequiredService<IIdGenerator<TTestEntity>>();

        /// <summary>
        /// Gets the test entity generator.
        /// </summary>
        protected abstract Faker<TTestEntity> TestEntityGenerator { get; }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldAdd()
        {
            var repo = GetRepositoryAs<IRepositoryAdd<TTestEntity>>();

            var entity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entity, cleanUpOnly: true);

            var result = await repo.AddAsync(entity);

            result.Id.Should().NotBeNullOrEmpty();
            result.ETag.Should().NotBeNullOrEmpty();
            result.Timestamp.Should().NotBeNull();
            result.Should().BeEquivalentTo(entity, opt => opt.Excluding(x => x.Id).Excluding(x => x.ETag).Excluding(x => x.Timestamp));
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotAddWhenNull()
        {
            try
            {
                var repo = GetRepositoryAs<IRepositoryAdd<TTestEntity>>();
                await repo.AddAsync(null!);
                throw new Exception("Should have thrown another exception before.");
            }
            catch (ArgumentNullException)
            {
                Assert.True(true);
            }
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldAddWithExistingId()
        {
            var repo = GetRepositoryAs<IRepositoryAdd<TTestEntity>>();
            var existingId = await IdGenerator.Generate(null);
            var entity = TestEntityGenerator
                .RuleFor(x => x.Id, existingId)
                .Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entity, cleanUpOnly: true);

            Func<Task> act = async () => await repo.AddAsync(null!);

            var result = await repo.AddAsync(entity);

            result.Id.Should().Be(existingId);
            result.ETag.Should().NotBeNull();
            result.Timestamp.Should().NotBeNull();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotAddWhenConflict()
        {
            var repo = GetRepositoryAs<IRepositoryAdd<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var newEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .Generate();

            Func<Task> act = async () => await repo.AddAsync(newEntity);

            act.Should().Throw<ConflictException>().Which.TargetId.Should().Be(newEntity.Id);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldAddOrUpdateWhenNew()
        {
            var repo = GetRepositoryAs<IRepositoryAddOrUpdate<TTestEntity>>();
            var entity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entity, cleanUpOnly: true);

            var (result, added) = await repo.AddOrUpdateAsync(entity);

            added.Should().BeTrue();
            result.Id.Should().NotBeNullOrEmpty();
            result.ETag.Should().NotBeNullOrEmpty();
            result.Timestamp.Should().NotBeNull();
            result.Should().BeEquivalentTo(entity, opt => opt.Excluding(x => x.Id).Excluding(x => x.ETag).Excluding(x => x.Timestamp));
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldAddOrUpdateWhenNotNewUnconditionally()
        {
            var repo = GetRepositoryAs<IRepositoryAddOrUpdate<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .Generate();

            var (result, added) = await repo.AddOrUpdateAsync(updatedEntity);

            added.Should().BeFalse();
            result.ETag.Should().NotBeNullOrEmpty();
            result.Timestamp.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedEntity, opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
            result.Should().NotBeEquivalentTo(existingEntity, opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldAddOrUpdateWhenNotNewConditionally()
        {
            var repo = GetRepositoryAs<IRepositoryAddOrUpdate<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .Generate();

            var (result, added) = await repo.AddOrUpdateAsync(updatedEntity);

            added.Should().BeFalse();
            result.ETag.Should().NotBeNullOrEmpty();
            result.Timestamp.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedEntity, opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
            result.Should().NotBeEquivalentTo(existingEntity, opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotAddOrUpdateWhenNull()
        {
            try
            {
                var repo = GetRepositoryAs<IRepositoryAddOrUpdate<TTestEntity>>();
                await repo.AddOrUpdateAsync(null!);
                throw new Exception("Should have thrown another exception before.");
            }
            catch (ArgumentNullException)
            {
                Assert.True(true);
            }
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotAddOrUpdateConditionallyWhenConcurrencyIssue()
        {
            var repo = GetRepositoryAs<IRepositoryAddOrUpdate<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .RuleFor(x => x.ETag, await IdGenerator.Generate())
                .Generate();

            try
            {
                await repo.AddOrUpdateAsync(updatedEntity);
                throw new Exception("Should not be reached.");
            }
            catch (ConcurrencyException concurrencyException)
            {
                concurrencyException.TargetId.Should().Be(existingEntity.Id);
            }
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldTryGetById()
        {
            var repo = GetRepositoryAs<IRepositoryGet<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);
            var referenceEntity = entities[1];

            var result = await repo.TryGetByIdAsync(referenceEntity.Id);

            result.Should().BeEquivalentTo(referenceEntity, opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldTryGetByIdWhenNull()
        {
            var repo = GetRepositoryAs<IRepositoryGet<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);
            var notExistingId = await IdGenerator.Generate();

            var result = await repo.TryGetByIdAsync(notExistingId);

            result.Should().BeNull();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldGetById()
        {
            var repo = GetRepositoryAs<IRepositoryGet<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);
            var referenceEntity = entities[1];

            var result = await repo.GetByIdAsync(referenceEntity.Id);

            result.Should().BeEquivalentTo(referenceEntity, opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldGetByIdWhenNotFound()
        {
            var repo = GetRepositoryAs<IRepositoryGet<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);
            var notExistingId = await IdGenerator.Generate();

            Func<Task> act = async () => await repo.GetByIdAsync(notExistingId);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(notExistingId);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldTryGetByIds()
        {
            var repo = GetRepositoryAs<IRepositoryGet<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);

            var result = await repo.TryGetByIdsAsync(entities.Select(x => x.Id));

            result.Should().HaveSameCount(entities);
            result.Keys.Should().Contain(entities.Select(x => x.Id));
            result[entities.First().Id].Should().BeEquivalentTo(entities.First(), opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldTryGetByIdsWhenSomeNotFound()
        {
            var repo = GetRepositoryAs<IRepositoryGet<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);
            var notExistingId = await IdGenerator.Generate();

            var result = await repo.TryGetByIdsAsync(entities.Select(x => x.Id).Union(new[] { notExistingId }));

            result.Should().HaveSameCount(entities);
            result.Keys.Should().Contain(entities.Select(x => x.Id));
            result.Keys.Should().NotContain(notExistingId);
            result.Keys.Should().NotContainNulls();
            result[entities.First().Id].Should().BeEquivalentTo(entities.First(), opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldExists()
        {
            var repo = GetRepositoryAs<IRepositoryGet<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);
            var referenceEntity = entities[1];

            var result = await repo.ExistsAsync(referenceEntity.Id);

            result.Should().BeTrue();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotExistsWhenNotFound()
        {
            var repo = GetRepositoryAs<IRepositoryGet<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(3);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);
            var notExistingId = await IdGenerator.Generate();

            var result = await repo.ExistsAsync(notExistingId);

            result.Should().BeFalse();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotExistsWhenNull()
        {
            try
            {
                var repo = GetRepositoryAs<IRepositoryGet<TTestEntity>>();
                await repo.ExistsAsync(null!);
                throw new Exception("Should have thrown another exception before.");
            }
            catch (ArgumentNullException)
            {
                Assert.True(true);
            }
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldUpdateUnconditionally()
        {
            var repo = GetRepositoryAs<IRepositoryUpdate<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .Generate();

            var result = await repo.UpdateAsync(updatedEntity);

            result.ETag.Should().NotBeNullOrEmpty();
            result.Timestamp.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedEntity, opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
            result.Should().NotBeEquivalentTo(existingEntity, opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldUpdateConditionally()
        {
            var repo = GetRepositoryAs<IRepositoryUpdate<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .RuleFor(x => x.ETag, existingEntity.ETag)
                .Generate();

            var result = await repo.UpdateAsync(updatedEntity);

            result.ETag.Should().NotBeNullOrEmpty();
            result.ETag.Should().NotBe(existingEntity.ETag);
            result.Timestamp.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedEntity, opt => opt.Excluding(x => x.ETag).Excluding(x => x.Timestamp));
            result.Should().NotBeEquivalentTo(existingEntity);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotUpdateWhenNull()
        {
            try
            {
                var repo = GetRepositoryAs<IRepositoryUpdate<TTestEntity>>();
                await repo.UpdateAsync(null!);
                throw new Exception("Should have thrown another exception before.");
            }
            catch (ArgumentNullException)
            {
                Assert.True(true);
            }
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotUpdateIfNotFound()
        {
            var repo = GetRepositoryAs<IRepositoryUpdate<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var notExistingId = await IdGenerator.Generate();
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, notExistingId)
                .Generate();

            Func<Task> act = async () => await repo.UpdateAsync(updatedEntity);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(notExistingId);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotUpdateConditionallyWhenConcurrencyIssue()
        {
            var repo = GetRepositoryAs<IRepositoryUpdate<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var updatedEntity = TestEntityGenerator
                .RuleFor(x => x.Id, existingEntity.Id)
                .RuleFor(x => x.ETag, await IdGenerator.Generate())
                .Generate();

            Func<Task> act = async () => await repo.UpdateAsync(updatedEntity);

            act.Should().Throw<ConcurrencyException>().Which.TargetId.Should().Be(existingEntity.Id);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldRemoveById()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);

            await repo.RemoveAsync(existingEntity.Id);

            (await GetRepositoryAs<IRepositoryGet<TTestEntity>>().TryGetByIdAsync(existingEntity.Id)).Should().BeNull();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotRemoveByIdWhenNotFound()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var notExistingId = await IdGenerator.Generate();

            Func<Task> act = async () => await repo.RemoveAsync(notExistingId);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(notExistingId);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotRemoveByIdWhenIdIsNull()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);

            Func<Task> act = async () => await repo.RemoveAsync((string)null!);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(null);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldRemoveByEntity()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);

            await repo.RemoveAsync(existingEntity);

            (await GetRepositoryAs<IRepositoryGet<TTestEntity>>().TryGetByIdAsync(existingEntity.Id)).Should().BeNull();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotRemoveByEntityWhenNotFound()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var notExistingEntity = TestEntityGenerator
                .RuleFor(x => x.Id, await IdGenerator.Generate())
                .Generate();

            Func<Task> act = async () => await repo.RemoveAsync(notExistingEntity);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(notExistingEntity.Id);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldNotRemoveByEntityWhenIdIsNull()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var notExistingEntity = TestEntityGenerator.Generate();

            Func<Task> act = async () => await repo.RemoveAsync(notExistingEntity);

            act.Should().Throw<NotFoundException>().Which.TargetId.Should().Be(notExistingEntity.Id);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldTryRemoveById()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);

            var result = await repo.TryRemoveAsync(existingEntity.Id);

            result.Should().BeTrue();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldTryRemoveByIdWhenNotFound()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var notExistingId = await IdGenerator.Generate();

            var result = await repo.TryRemoveAsync(notExistingId);

            result.Should().BeFalse();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldTryRemoveByIdWhenIdIsNull()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);

            var result = await repo.TryRemoveAsync((string)null!);

            result.Should().BeFalse();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldTryRemoveByEntity()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);

            var result = await repo.TryRemoveAsync(existingEntity);

            result.Should().BeTrue();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldTryRemoveByEntityWhenNotFound()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var notExistingEntity = TestEntityGenerator
                .RuleFor(x => x.Id, await IdGenerator.Generate())
                .Generate();

            var result = await repo.TryRemoveAsync(notExistingEntity);

            result.Should().BeFalse();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldTryRemoveByEntityWhenIdIsNull()
        {
            var repo = GetRepositoryAs<IRepositoryRemove<TTestEntity>>();
            var existingEntity = TestEntityGenerator.Generate();
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, existingEntity);
            var notExistingEntity = TestEntityGenerator.Generate();

            var result = await repo.TryRemoveAsync(notExistingEntity);

            result.Should().BeFalse();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldFindAll()
        {
            var repo = GetRepositoryAs<IRepositoryFindAll<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);

            var result = await repo.FindAllAsync(null);

            result.Count().Should().BeGreaterOrEqualTo(entities.Count);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldFindAllByPage()
        {
            var repo = GetRepositoryAs<IRepositoryFindAll<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);

            var firstResult = await repo.FindAllAsync(null, limit: 3);

            firstResult.Count().Should().Be(3);
            firstResult.ContinuationToken.Should().NotBeNullOrEmpty();

            var secondResult = await repo.FindAllAsync(firstResult.ContinuationToken, limit: 3);

            secondResult.Count().Should().BeLessOrEqualTo(3);
            secondResult.ContinuationToken.Should().NotBe(firstResult.ContinuationToken);
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldFindAllWithZero()
        {
            var repo = GetRepositoryAs<IRepositoryFindAll<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);

            var result = await repo.FindAllAsync(null, 0);

            result.Should().BeEmpty();
            result.ContinuationToken.Should().BeNullOrEmpty();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldFindAllWithNegativeNumber()
        {
            var repo = GetRepositoryAs<IRepositoryFindAll<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);

            var result = await repo.FindAllAsync(null, -3);

            result.Should().BeEmpty();
            result.ContinuationToken.Should().BeNullOrEmpty();
        }

        [SkippableFact(typeof(UnsupportedFeatureException))]
        public virtual async Task ItShouldClear()
        {
            var repo = GetRepositoryAs<IRepositoryClear<TTestEntity>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<TTestEntity>(repo, entities);

            await repo.ClearAsync();

            var result = await GetRepositoryAs<IRepositoryFindAll<TTestEntity>>().FindAllAsync(null);

            result.Should().BeEmpty();
        }

        protected TInterface GetRepositoryAs<TInterface>()
            where TInterface : class
        {
            Skip.IfNot(Repository is TInterface);
            return (Repository as TInterface) !;
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
