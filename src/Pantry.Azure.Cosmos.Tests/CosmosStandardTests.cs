using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;
using Xunit.Abstractions;

namespace Pantry.Azure.Cosmos.Tests
{
    public class CosmosStandardTests : StandardRepositoryImplementationTests<CosmosRepository<StandardEntity>>
    {
        private const string CosmosConnectionString = nameof(CosmosConnectionString);

        public CosmosStandardTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override void RegisterTestServices<TEntity>(HostBuilderContext context, IServiceCollection services)
        {
            services
                .AddCosmosRepository<TEntity>()
                .WithConnectionStringNamed(CosmosConnectionString);
        }

        protected override IEnumerable<KeyValuePair<string, string>> AdditionalConfigurationValues()
            => new Dictionary<string, string>
            {
                { $"ConnectionStrings:{CosmosConnectionString}", "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" },
            };
    }
}
