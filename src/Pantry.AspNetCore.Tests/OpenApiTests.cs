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
        public async Task ItShouldGetOpenApi()
        {
            var result = await Factory.CreateClient().GetStringAsync("/swagger/v1/swagger.json");
            result.Should().NotBeNullOrEmpty();
            // result.Should().NotContain("/api/create-controller/entities/{id}");
        }
    }
}
