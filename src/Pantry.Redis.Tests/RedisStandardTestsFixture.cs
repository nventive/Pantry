using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.Redis.Tests
{
    public class RedisStandardTestsFixture : StandardRepositoryImplementationTestsFixture
    {
        private const string RedisConnectionString = nameof(RedisConnectionString);

        protected override void RegisterTestServices(HostBuilderContext context, IServiceCollection services)
        {
            services
                .AddRedisRepository<StandardEntity>()
                .WithConnectionStringNamed(RedisConnectionString);

            services
                .AddHealthChecks()
                .AddRedisRepositoryCheck<StandardEntity>();
        }

        protected override IEnumerable<KeyValuePair<string, string>> AdditionalConfigurationValues()
            => new Dictionary<string, string>
            {
                { $"ConnectionStrings:{RedisConnectionString}", "localhost" },
            };
    }
}
