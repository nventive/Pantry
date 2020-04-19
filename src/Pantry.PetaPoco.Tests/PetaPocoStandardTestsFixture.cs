using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.PetaPoco.Tests
{
    public class PetaPocoStandardTestsFixture : StandardRepositoryImplementationTestsFixture
    {
        protected override void RegisterTestServices(HostBuilderContext context, IServiceCollection services)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            services.AddPetaPocoRepository<StandardEntity>();

            services
                .AddHealthChecks()
                .AddPetaPocoRepositoryCheck<StandardEntity>();
        }
    }
}
