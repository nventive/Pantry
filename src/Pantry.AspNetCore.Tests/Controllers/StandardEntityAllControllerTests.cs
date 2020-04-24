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
    public class StandardEntityAllControllerTests : WebTests
    {
        public StandardEntityAllControllerTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
            : base(factory, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldCreate()
        {
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");
            var attributes = StandardEntityCreateModelGenerator.Generate();

            var result = await client.Create(attributes);

            result.StatusCode.Should().Be(HttpStatusCode.Created);
            result.Headers.Location.AbsoluteUri.Should().NotBeNullOrEmpty();

            AssertEntityAttributesAreOk(result.Content, attributes);
        }

        [Fact]
        public async Task ItShouldCreateAndGet()
        {
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");
            var attributes = StandardEntityCreateModelGenerator.Generate();

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
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");

            var result = await client.GetById(entity.Id);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public async Task ItShouldFindAll()
        {
            await Factory.Services.GetRequiredService<IRepositoryClear<StandardEntity>>().ClearAsync();
            var entities = StandardEntityGenerator.Generate(2);
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entities[0]);
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entities[1]);
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");

            var result = await client.Find(new StandardEntityCriteriaQueryModel { Limit = 1 });

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Items.Should().HaveCount(1);
            result.Content.ContinuationToken.Should().NotBeNullOrEmpty();

            result = await client.Find(new StandardEntityCriteriaQueryModel { ContinuationToken = result.Content.ContinuationToken, Limit = 1 });

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Items.Should().HaveCount(1);
            result.Content.ContinuationToken.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task ItShouldGetNotModifiedIfModifiedSince()
        {
            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");

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
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");

            var result = await client.GetById(entity.Id);

            result = await client.GetById(
                entity.Id,
                ifNoneMatch: result.Headers.ETag.ToString());

            result.StatusCode.Should().Be(HttpStatusCode.NotModified);
        }

        [Fact]
        public async Task ItShouldNotGetIfNotFound()
        {
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");

            var result = await client.GetById(StandardEntityGenerator.Generate().Id);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ItShouldCreateAndUpdateUnconditionally()
        {
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");
            var attributes = StandardEntityCreateModelGenerator.Generate();
            var updatedAttributes = StandardEntityUpdateModelGenerator.Generate();

            var createResult = await client.Create(attributes);
            var result = await client.Update(createResult.Content.Id, updatedAttributes);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            AssertEntityAttributesAreOk(result.Content, updatedAttributes);
        }

        [Fact]
        public async Task ItShouldCreateAndUpdateConditionally()
        {
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");
            var attributes = StandardEntityCreateModelGenerator.Generate();
            var updatedAttributes = StandardEntityUpdateModelGenerator.Generate();

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
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");

            var result = await client.Update(
                StandardEntityGenerator.Generate().Id,
                StandardEntityUpdateModelGenerator.Generate());

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ItShouldNotUpdateIfPreconditionFailed()
        {
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");
            var attributes = StandardEntityCreateModelGenerator.Generate();
            var updatedAttributes = StandardEntityUpdateModelGenerator.Generate();

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
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");
            var attributes = StandardEntityCreateModelGenerator.Generate();

            var createResult = await client.Create(attributes);
            var result = await client.Delete(createResult.Content.Id);

            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ItShouldNotDeleteIfNotFound()
        {
            var client = GetRepositoryApiClient<StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQueryModel>("/api/standard-entities-all");

            var result = await client.Delete(StandardEntityGenerator.Generate().Id);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
