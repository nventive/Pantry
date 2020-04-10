using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;
using Xunit.Abstractions;

namespace Pantry.Dapper.Tests
{
    public class DapperStandardTests : StandardRepositoryImplementationTests
    {
        private const string DapperConnectionString = nameof(DapperConnectionString);

        public DapperStandardTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override void RegisterTestServices<TEntity>(HostBuilderContext context, IServiceCollection services)
        {
            services
                .AddDapperRepository<TEntity>()
                .WithConnectionStringNamed(SqlClientFactory.Instance, DapperConnectionString);
        }

        protected override IEnumerable<KeyValuePair<string, string>> AdditionalConfigurationValues()
            => new Dictionary<string, string>
            {
                { $"ConnectionStrings:{DapperConnectionString}", "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=PantryDapper;" },
            };
    }
}
