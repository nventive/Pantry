﻿using System.Globalization;
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
    public class GetControllerTests : WebTests
    {
        public GetControllerTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
            : base(factory, outputHelper)
        {
        }

        [Fact]
        public async Task ItShouldGet()
        {
            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);
            var client = GetControllerApiClient("/api/get-controller/entities");

            var result = await client.GetById(entity.Id);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public async Task ItShouldGetNotModifiedIfModifiedSince()
        {
            var entity = StandardEntityGenerator.Generate();
            await Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);
            var client = GetControllerApiClient("/api/get-controller/entities");

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
            var client = GetControllerApiClient("/api/get-controller/entities");

            var result = await client.GetById(
                entity.Id,
                ifNoneMatch: new EntityTagHeaderValue($"\"{entity.ETag}\"", true).ToString());

            result.StatusCode.Should().Be(HttpStatusCode.NotModified);
        }

        [Fact]
        public async Task ItShouldNotGetIfNotFound()
        {
            var client = GetControllerApiClient("/api/get-controller/entities");

            var result = await client.GetById(StandardEntityGenerator.Generate().Id);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
