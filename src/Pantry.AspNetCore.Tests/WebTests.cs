using System;
using System.Globalization;
using Bogus;
using FluentAssertions;
using Pantry.AspNetCore.Tests.Server;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.AspNetCore.Tests
{
    /// <summary>
    /// Base class for tests that uses the <see cref="TestWebApplicationFactory"/>.
    /// </summary>
    [Collection(TestWebApplicationFactoryCollection.CollectionName)]
    public abstract class WebTests
    {
        protected WebTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Factory.OutputHelper = outputHelper;
        }

        /// <summary>
        /// Gets the <see cref="TestWebApplicationFactory"/>.
        /// </summary>
        protected TestWebApplicationFactory Factory { get; }

        protected Faker<StandardEntityAttributes> StandardEntityAttributesGenerator => new Faker<StandardEntityAttributes>()
            .RuleFor(x => x.Name, f => f.Person.UserName)
            .RuleFor(x => x.Age, f => f.Random.Int(1, 100));

        protected Faker<StandardEntity> StandardEntityGenerator => new Faker<StandardEntity>()
            .RuleFor(x => x.Id, f => f.Random.Guid().ToString("n", CultureInfo.InvariantCulture))
            .RuleFor(x => x.ETag, f => f.Random.Hash())
            .RuleFor(x => x.Timestamp, f => f.Date.PastOffset())
            .RuleFor(x => x.Name, f => f.Person.UserName)
            .RuleFor(x => x.Age, f => f.Random.Int(1, 100));

        protected IControllerApiClient<StandardEntity, StandardEntityAttributes> GetControllerApiClient(string relativeUri)
            => Factory.GetApiClient<IControllerApiClient<StandardEntity, StandardEntityAttributes>>(relativeUri);

        protected void AssertEntityAttributesAreOk(StandardEntity entity, StandardEntityAttributes attributes)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (attributes is null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            entity.Id.Should().NotBeNullOrEmpty();
            entity.ETag.Should().NotBeNullOrEmpty();
            entity.Timestamp.Should().NotBeNull();
            entity.Name.Should().Be(attributes.Name);
            entity.Age.Should().Be(attributes.Age);
        }
    }
}
