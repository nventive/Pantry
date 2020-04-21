using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pantry.AspNetCore.Tests.Server;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.AspNetCore.Tests.Controllers
{
    public class ETagsTests : WebTests
    {
        public ETagsTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
            : base(factory, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldAllowWeakETagsFromRepository()
        {
            var entity = StandardEntityGenerator
                .RuleFor(x => x.ETag, "W/\"123456\"")
                .Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);
            var client = GetRepositoryApiClient("/api/standard-entities-all");

            var result = await client.GetById(entity.Id);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Should().BeEquivalentTo(entity);

            result = await client.GetById(entity.Id, ifNoneMatch: result.Headers.ETag.ToString());
            result.StatusCode.Should().Be(HttpStatusCode.NotModified);
        }
    }
}
