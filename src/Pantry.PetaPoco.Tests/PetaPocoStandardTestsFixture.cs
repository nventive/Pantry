using System;
using System.IO;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantry.Tests.StandardTestSupport;

namespace Pantry.PetaPoco.Tests
{
    public class PetaPocoStandardTestsFixture : StandardRepositoryImplementationTestsFixture
    {
        private static readonly string TestDbPath = Path.GetFullPath(".\\Pantry.PetaPoco.Tests.db");

        protected override void RegisterTestServices(HostBuilderContext context, IServiceCollection services)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            services
                .AddFluentMigratorCore()
                .ConfigureRunner(runner =>
                {
                    runner
                        .AddSQLite()
                        .WithGlobalConnectionString($"Data Source={TestDbPath}")
                        .ScanIn(typeof(PetaPocoStandardTestsFixture).Assembly).For.Migrations();
                })
                .AddPetaPocoRepository<StandardEntity>()
                .WithConnectionString(SqliteFactory.Instance, $"Data Source={TestDbPath}");

            services
                .AddHealthChecks()
                .AddPetaPocoRepositoryCheck<StandardEntity>();
        }

        protected override void PostHostBuilt(IHost host)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (File.Exists(TestDbPath))
            {
                File.Delete(TestDbPath);
            }

            var runner = host.Services.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
    }
}
