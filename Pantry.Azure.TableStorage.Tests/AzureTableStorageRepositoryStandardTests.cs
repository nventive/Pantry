using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;
using Xunit.Abstractions;

namespace Pantry.Azure.TableStorage.Tests
{
    public class AzureTableStorageRepositoryStandardTests : StandardRepositoryImplementationTests
    {
        private const string StorageConnectionString = nameof(StorageConnectionString);

        public AzureTableStorageRepositoryStandardTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override void RegisterTestServices<TEntity>(HostBuilderContext context, IServiceCollection services)
        {
            services
                .AddAzureTableStorageRepository<TEntity>()
                .WithConnectionStringNamed(StorageConnectionString);
        }

        protected override IEnumerable<KeyValuePair<string, string>> AdditionalConfigurationValues()
            => new Dictionary<string, string>
            {
                { $"ConnectionStrings:{StorageConnectionString}", "UseDevelopmentStorage=true" },
            };
    }
}
