using System;
using Bogus;
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
            .RuleFor(x => x.Name, f => f.Person.UserName)
            .RuleFor(x => x.Age, f => f.Random.Int(1, 100));
    }
}
