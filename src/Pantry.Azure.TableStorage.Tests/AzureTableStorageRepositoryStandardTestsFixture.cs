using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.Azure.TableStorage.Tests
{
    public class AzureTableStorageRepositoryStandardTestsFixture : StandardRepositoryImplementationTestsFixture
    {
        private const string StorageConnectionString = nameof(StorageConnectionString);

        protected override void RegisterTestServices(HostBuilderContext context, IServiceCollection services)
        {
            services
                .AddAzureTableStorageRepository<StandardEntity>()
                .WithConnectionStringNamed(StorageConnectionString);
        }

        protected override IEnumerable<KeyValuePair<string, string>> AdditionalConfigurationValues()
            => new Dictionary<string, string>
            {
                { $"ConnectionStrings:{StorageConnectionString}", "UseDevelopmentStorage=true" },
            };
    }
}
