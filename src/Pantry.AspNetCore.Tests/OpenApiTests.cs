using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.AspNetCore.Tests
{
    public class OpenApiTests : WebTests
    {
        public OpenApiTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
            : base(factory, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldGetSwagger()
        {
            var result = await Factory.CreateClient().GetStringAsync("/swagger/v1/swagger.json");
            result.Should().NotBeNullOrEmpty();
        }
    }
}
