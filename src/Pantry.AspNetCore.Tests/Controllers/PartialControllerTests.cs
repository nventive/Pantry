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
    public class PartialControllerTests : WebTests
    {
        public PartialControllerTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
            : base(factory, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldOnlyExposeCreate()
        {
            var client = GetRepositoryApiClient("/api/standard-entities-create");
            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var result = await client.Create(StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            result = await client.GetById(entity.Id);
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var collectionResult = await client.FindAll();
            collectionResult.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);

            result = await client.Update(
                entity.Id,
                StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var response = await client.Delete(entity.Id);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ItShouldOnlyExposeGet()
        {
            var client = GetRepositoryApiClient("/api/standard-entities-get");
            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var result = await client.Create(StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await client.GetById(entity.Id);
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var collectionResult = await client.FindAll();
            collectionResult.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await client.Update(
                entity.Id,
                StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);

            var response = await client.Delete(entity.Id);
            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        }

        [Fact]
        public async Task ItShouldOnlyExposeUpdate()
        {
            var client = GetRepositoryApiClient("/api/standard-entities-update");

            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var result = await client.Create(StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await client.GetById(entity.Id);
            result.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);

            var collectionResult = await client.FindAll();
            collectionResult.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await client.Update(
                entity.Id,
                StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var response = await client.Delete(entity.Id);
            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        }

        [Fact]
        public async Task ItShouldOnlyExposeDelete()
        {
            var client = GetRepositoryApiClient("/api/standard-entities-delete");

            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var result = await client.Create(StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await client.GetById(entity.Id);
            result.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);

            var collectionResult = await client.FindAll();
            collectionResult.StatusCode.Should().Be(HttpStatusCode.NotFound);

            result = await client.Update(
                entity.Id,
                StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);

            var response = await client.Delete(entity.Id);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ItShouldOnlyExposeCrud()
        {
            var client = GetRepositoryApiClient("/api/standard-entities-crud");

            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var result = await client.Create(StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.Created);

            result = await client.GetById(entity.Id);
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var collectionResult = await client.FindAll();
            collectionResult.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);

            result = await client.Update(
                entity.Id,
                StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var response = await client.Delete(entity.Id);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ItShouldOnlyExposeAll()
        {
            var client = GetRepositoryApiClient("/api/standard-entities-all");

            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var result = await client.Create(StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.Created);

            result = await client.GetById(entity.Id);
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var collectionResult = await client.FindAll();
            collectionResult.StatusCode.Should().Be(HttpStatusCode.OK);

            result = await client.Update(
                entity.Id,
                StandardEntityAttributesGenerator.Generate());
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var response = await client.Delete(entity.Id);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
