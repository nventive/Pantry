using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.Azure.Cosmos.Tests
{
    public class CosmosStandardTestsFixture : StandardRepositoryImplementationTestsFixture
    {
        public const string CosmosConnectionString = nameof(CosmosConnectionString);

        protected override void RegisterTestServices(HostBuilderContext context, IServiceCollection services)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            var connectionString = context.Configuration.GetConnectionString(CosmosConnectionString);
#pragma warning disable CA2000 // Dispose objects before losing scope
            var client = new CosmosClient(connectionString);
            client.CreateDatabaseIfNotExistsAsync("pantry-tests").ConfigureAwait(false).GetAwaiter().GetResult();
            var database = client.GetDatabase("pantry-tests");
            database.CreateContainerIfNotExistsAsync("pantry-tests", "/id", 400).ConfigureAwait(false).GetAwaiter().GetResult();

            services
                .AddCosmosRepository<StandardEntity>()
                .WithCosmosContainerFactory(sp => database.GetContainer("pantry-tests"));
        }

        protected override IEnumerable<KeyValuePair<string, string>> AdditionalConfigurationValues()
            => new Dictionary<string, string>
            {
                { $"ConnectionStrings:{CosmosConnectionString}", "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" },
            };
    }
}
