using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pantry.AspNetCore.Tests.Server;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.AspNetCore.Tests.Controllers
{
    public class StandardEntityCRUDControllerTests : WebTests
    {
        public StandardEntityCRUDControllerTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
            : base(factory, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldCreate()
        {
            var client = GetRepositoryApiClient("/api/standard-entities");
            var attributes = StandardEntityAttributesGenerator.Generate();

            var result = await client.Create(attributes);

            result.StatusCode.Should().Be(HttpStatusCode.Created);
            result.Headers.Location.AbsoluteUri.Should().NotBeNullOrEmpty();

            AssertEntityAttributesAreOk(result.Content, attributes);
        }

        [Fact]
        public async Task ItShouldCreateAndGet()
        {
            var client = GetRepositoryApiClient("/api/standard-entities");
            var attributes = StandardEntityAttributesGenerator.Generate();

            var createResult = await client.Create(attributes);
            var result = await client.GetById(createResult.Content.Id);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            AssertEntityAttributesAreOk(result.Content, attributes);
        }

        [Fact]
        public async Task ItShouldGet()
        {
            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);
            var client = GetRepositoryApiClient("/api/standard-entities");

            var result = await client.GetById(entity.Id);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public async Task ItShouldGetNotModifiedIfModifiedSince()
        {
            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);
            var client = GetRepositoryApiClient("/api/standard-entities");

            var result = await client.GetById(
                entity.Id,
                ifModifiedSince: entity.Timestamp!.Value.ToString("r", CultureInfo.InvariantCulture));

            result.StatusCode.Should().Be(HttpStatusCode.NotModified);
        }

        [Fact]
        public async Task ItShouldGetNotModifiedIfNoneMatch()
        {
            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);
            var client = GetRepositoryApiClient("/api/standard-entities");

            var result = await client.GetById(
                entity.Id,
                ifNoneMatch: new EntityTagHeaderValue($"\"{entity.ETag}\"", true).ToString());

            result.StatusCode.Should().Be(HttpStatusCode.NotModified);
        }

        [Fact]
        public async Task ItShouldNotGetIfNotFound()
        {
            var client = GetRepositoryApiClient("/api/standard-entities");

            var result = await client.GetById(StandardEntityGenerator.Generate().Id);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ItShouldCreateAndUpdateUnconditionally()
        {
            var client = GetRepositoryApiClient("/api/standard-entities");
            var attributes = StandardEntityAttributesGenerator.Generate();
            var updatedAttributes = StandardEntityAttributesGenerator.Generate();

            var createResult = await client.Create(attributes);
            var result = await client.Update(createResult.Content.Id, updatedAttributes);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            AssertEntityAttributesAreOk(result.Content, updatedAttributes);
        }

        [Fact]
        public async Task ItShouldCreateAndUpdateConditionally()
        {
            var client = GetRepositoryApiClient("/api/standard-entities");
            var attributes = StandardEntityAttributesGenerator.Generate();
            var updatedAttributes = StandardEntityAttributesGenerator.Generate();

            var createResult = await client.Create(attributes);
            var result = await client.Update(
                createResult.Content.Id,
                updatedAttributes,
                ifMatch: createResult.Headers.ETag.ToString());

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            AssertEntityAttributesAreOk(result.Content, updatedAttributes);
        }

        [Fact]
        public async Task ItShouldNotUpdateIfNotFound()
        {
            var client = GetRepositoryApiClient("/api/standard-entities");

            var result = await client.Update(
                StandardEntityGenerator.Generate().Id,
                StandardEntityAttributesGenerator.Generate());

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ItShouldNotUpdateIfPreconditionFailed()
        {
            var client = GetRepositoryApiClient("/api/standard-entities");
            var attributes = StandardEntityAttributesGenerator.Generate();
            var updatedAttributes = StandardEntityAttributesGenerator.Generate();

            var createResult = await client.Create(attributes);
            var result = await client.Update(
                createResult.Content.Id,
                updatedAttributes,
                ifMatch: new EntityTagHeaderValue("\"wrongtag\"", true).ToString());

            result.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        }

        [Fact]
        public async Task ItShouldCreateAndDelete()
        {
            var client = GetRepositoryApiClient("/api/standard-entities");
            var attributes = StandardEntityAttributesGenerator.Generate();

            var createResult = await client.Create(attributes);
            var result = await client.Delete(createResult.Content.Id);

            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ItShouldNotDeleteIfNotFound()
        {
            var client = GetRepositoryApiClient("/api/standard-entities");

            var result = await client.Delete(StandardEntityGenerator.Generate().Id);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
