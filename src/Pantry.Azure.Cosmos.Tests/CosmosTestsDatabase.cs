using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Pantry.Azure.Cosmos.Tests
{
    public class CosmosTestsDatabase
    {
        private readonly Lazy<Database> _lazyDatabase;

        public CosmosTestsDatabase()
        {
            _lazyDatabase = new Lazy<Database>(() =>
            {
                var connectionString = Configuration.GetConnectionString(CosmosStandardTests.CosmosConnectionString);
#pragma warning disable CA2000 // Dispose objects before losing scope
                var client = new CosmosClient(connectionString);
                client.CreateDatabaseIfNotExistsAsync("pantry-tests").ConfigureAwait(false).GetAwaiter().GetResult();
                var database = client.GetDatabase("pantry-tests");
                database.CreateContainerIfNotExistsAsync("pantry-tests", "/id", 400).ConfigureAwait(false).GetAwaiter().GetResult();

                return database;
            });
        }

        public IConfiguration? Configuration { get; set; }

        public Database GetDatabase() => _lazyDatabase.Value;
    }
}
