using System.Threading.Tasks;
using FluentAssertions;
using Pantry.AspNetCore.Tests.Server;
using Refit;
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

        internal interface ICreateControllerApi
        {
            [Post("/api/create-controller/entities")]
            Task<StandardEntity> Create([Body] StandardEntityAttributes attributes);
        }

        [Fact]
        public async Task ItShouldCreate()
        {
            var client = Factory.GetApiClient<ICreateControllerApi>();
            var attributes = StandardEntityAttributesGenerator.Generate();

            var result = await client.Create(attributes);

            result.Id.Should().NotBeNullOrEmpty();
            result.ETag.Should().NotBeNullOrEmpty();
            result.Timestamp.Should().NotBeNull();
            result.Name.Should().Be(attributes.Name);
            result.Age.Should().Be(attributes.Age);
        }
    }
}
