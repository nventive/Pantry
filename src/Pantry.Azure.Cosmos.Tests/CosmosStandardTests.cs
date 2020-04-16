using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Azure.Cosmos.Tests
{
    [Collection(CosmosTestsDatabaseCollection.CollectionName)]
    public class CosmosStandardTests : StandardRepositoryImplementationTests<CosmosRepository<StandardEntity>>
    {
        public const string CosmosConnectionString = nameof(CosmosConnectionString);
        private readonly CosmosTestsDatabase _testsDatabase;

        public CosmosStandardTests(CosmosTestsDatabase testsDatabase, ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            _testsDatabase = testsDatabase ?? throw new ArgumentNullException(nameof(testsDatabase));
        }

        protected override void RegisterTestServices<TEntity>(HostBuilderContext context, IServiceCollection services)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _testsDatabase.Configuration = context.Configuration;
            services
                .AddCosmosRepository<TEntity>()
                .WithCosmosContainerFactory(sp => _testsDatabase.GetDatabase().GetContainer("pantry-tests"));
        }

        protected override IEnumerable<KeyValuePair<string, string>> AdditionalConfigurationValues()
            => new Dictionary<string, string>
            {
                { $"ConnectionStrings:{CosmosConnectionString}", "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" },
            };
    }
}
