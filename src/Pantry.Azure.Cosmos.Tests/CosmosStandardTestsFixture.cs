using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Azure.Cosmos.Configuration;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.Azure.Cosmos.Tests
{
    public class CosmosStandardTestsFixture : StandardRepositoryImplementationTestsFixture
    {
        protected override void RegisterTestServices(HostBuilderContext context, IServiceCollection services)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            services.AddCosmosRepository<StandardEntity>();

            services
                .AddHealthChecks()
                .AddCosmosRepositoryCheck<StandardEntity>();
        }

        protected override IEnumerable<KeyValuePair<string, string>> AdditionalConfigurationValues()
            => new Dictionary<string, string>
            {
                { $"ConnectionStrings:{CosmosRepositoryOptions.DefaultConnectionStringName}", "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" },
            };
    }
}
