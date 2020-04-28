using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Mediator.AspNetCore.Tests
{
    [Collection(TestWebApplicationFactoryCollection.CollectionName)]
    public class OpenApiTests
    {
        public OpenApiTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Factory.OutputHelper = outputHelper;
        }

        /// <summary>
        /// Gets the <see cref="TestWebApplicationFactory"/>.
        /// </summary>
        private TestWebApplicationFactory Factory { get; }

        [Fact]
        public async Task ItShouldGetOpenApi()
        {
            var result = await Factory.CreateClient().GetStringAsync("/swagger/v1/swagger.json");
            result.Should().NotBeNullOrEmpty();
        }
    }
}
