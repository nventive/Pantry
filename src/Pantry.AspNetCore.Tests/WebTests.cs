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

        protected Faker<StandardEntityCreateModel> StandardEntityCreateModelGenerator => new Faker<StandardEntityCreateModel>()
            .RuleFor(x => x.Name, f => f.Person.UserName)
            .RuleFor(x => x.Age, f => f.Random.Int(1, 100));

        protected Faker<StandardEntityUpdateModel> StandardEntityUpdateModelGenerator => new Faker<StandardEntityUpdateModel>()
            .RuleFor(x => x.Name, f => f.Person.UserName);

        protected Faker<StandardEntity> StandardEntityGenerator => new Faker<StandardEntity>()
            .RuleFor(x => x.Id, f => f.Random.Guid().ToString("n", CultureInfo.InvariantCulture))
            .RuleFor(x => x.ETag, f => f.Random.Hash())
            .RuleFor(x => x.Timestamp, f => f.Date.PastOffset())
            .RuleFor(x => x.Name, f => f.Person.UserName)
            .RuleFor(x => x.Age, f => f.Random.Int(1, 100));

        protected IRepositoryApiClient<TResultModel, TResultsModel, TCreateModel, TUpdateModel, TQueryModel> GetRepositoryApiClient<TResultModel, TResultsModel, TCreateModel, TUpdateModel, TQueryModel>(string relativeUri)
            => Factory.GetApiClient<IRepositoryApiClient<TResultModel, TResultsModel, TCreateModel, TUpdateModel, TQueryModel>>(relativeUri);

        protected void AssertEntityAttributesAreOk(StandardEntityModel entity, StandardEntityCreateModel attributes)
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
            entity.Name.Should().Be(attributes.Name);
            entity.Age.Should().Be(attributes.Age);
        }

        protected void AssertEntityAttributesAreOk(StandardEntityModel entity, StandardEntityUpdateModel attributes)
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
            entity.Name.Should().Be(attributes.Name);
        }
    }
}
