using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Pantry.AspNetCore.Tests.Server;
using Refit;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.AspNetCore.Tests.Controllers
{
    public class CreateGetControllerTests : WebTests
    {
        public CreateGetControllerTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
            : base(factory, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldCreate()
        {
            var client = GetControllerApiClient("/api/create-get-controller/entities");
            var attributes = StandardEntityAttributesGenerator.Generate();

            var result = await client.Create(attributes);

            result.StatusCode.Should().Be(HttpStatusCode.Created);
            result.Headers.Location.AbsoluteUri.Should().NotBeNullOrEmpty();

            AssertEntityAttributesAreOk(result.Content, attributes);
        }

        [Fact]
        public async Task ItShouldCreateAndGet()
        {
            var client = GetControllerApiClient("/api/create-get-controller/entities");
            var attributes = StandardEntityAttributesGenerator.Generate();

            var createResult = await client.Create(attributes);
            var result = await client.GetById(createResult.Content.Id);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            AssertEntityAttributesAreOk(result.Content, attributes);
        }
    }
}
