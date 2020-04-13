using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.AspNetCore.Tests.Controllers
{
    public class CreateControllerTests : WebTests
    {
        public CreateControllerTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
            : base(factory, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldCreate()
        {
            var client = GetControllerApiClient("/api/create-controller/entities");
            var attributes = StandardEntityAttributesGenerator.Generate();

            var result = await client.Create(attributes);
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            AssertEntityAttributesAreOk(result.Content, attributes);
        }
    }
}
