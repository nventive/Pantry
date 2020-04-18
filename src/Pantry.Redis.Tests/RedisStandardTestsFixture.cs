using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.Redis.Tests
{
    public class RedisStandardTestsFixture : StandardRepositoryImplementationTestsFixture
    {
        protected override void RegisterTestServices(HostBuilderContext context, IServiceCollection services)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            services.AddRedisRepository<StandardEntity>();
        }
    }
}
