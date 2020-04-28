using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pantry.Mediator.AspNetCore.Tests.Server;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Mediator.AspNetCore.Tests
{
    [Collection(TestWebApplicationFactoryCollection.CollectionName)]
    public class ServerTests
    {
        public ServerTests(TestWebApplicationFactory factory, ITestOutputHelper outputHelper)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Factory.OutputHelper = outputHelper;
        }

        /// <summary>
        /// Gets the <see cref="TestWebApplicationFactory"/>.
        /// </summary>
        private TestWebApplicationFactory Factory { get; }

        private Faker<StandardEntity> StandardEntityGenerator => new Faker<StandardEntity>()
            .RuleFor(x => x.Id, f => f.Random.Guid().ToString("n", CultureInfo.InvariantCulture))
            .RuleFor(x => x.ETag, f => f.Random.Hash())
            .RuleFor(x => x.Timestamp, f => f.Date.PastOffset())
            .RuleFor(x => x.Name, f => f.Person.UserName)
            .RuleFor(x => x.Age, f => f.Random.Int(1, 100));

        [Fact]
        public async Task ItShouldGetById()
        {
            var entity = StandardEntityGenerator.Generate();
            var repoAdd = Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>();
            entity = await repoAdd.AddAsync(entity);

            var client = Factory.GetApiClient<IServerApi>();

            var result = await client.GetStandardEntityById(entity.Id);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public async Task ItShouldGetByIdWhenNotFound()
        {
            var entity = StandardEntityGenerator.Generate();
            var client = Factory.GetApiClient<IServerApi>();

            var result = await client.GetStandardEntityById(entity.Id);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ItShouldFind()
        {
            var entities = StandardEntityGenerator.Generate(10);
            var repoAdd = Factory.Services.GetRequiredService<IRepositoryAdd<StandardEntity>>();
            foreach (var entity in entities)
            {
                await repoAdd.AddAsync(entity);
            }

            var client = Factory.GetApiClient<IServerApi>();
            var query = new FindStandardEntityQuery { NameEq = entities.Last().Name };

            var result = await client.FindStandardEntities(query);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Content.Items.First().Should().BeEquivalentTo(entities.Last());
        }
    }
}
