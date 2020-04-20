using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Pantry.Azure.Cosmos.Configuration;

namespace Pantry.Azure.Cosmos
{
    /// <summary>
    /// CosmosDb default container factory.
    /// </summary>
    public class CosmosContainerFactory
    {
        private readonly Lazy<CosmosClient> _lazyClient;
        private readonly Lazy<Database> _lazyDatabase;
        private readonly string _connectionString;
        private readonly CosmosRepositoryOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosContainerFactory"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="options">The <see cref="CosmosRepositoryOptions"/>.</param>
        public CosmosContainerFactory(
            string connectionString,
            IOptions<CosmosRepositoryOptions> options)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _lazyClient = new Lazy<CosmosClient>(() => new CosmosClient(_connectionString));
            _lazyDatabase = new Lazy<Database>(BuildDatabase);
        }

        /// <summary>
        /// Gets the <see cref="CosmosClient"/>.
        /// </summary>
        /// <returns>The <see cref="CosmosClient"/>.</returns>
        public CosmosClient Client => _lazyClient.Value;

        /// <summary>
        /// Gets the <see cref="Database"/>.
        /// </summary>
        /// <returns>The <see cref="Database"/>.</returns>
        public Database Database => _lazyDatabase.Value;

        /// <summary>
        /// Gets a <see cref="Container"/>.
        /// </summary>
        /// <returns>The <see cref="Container"/>.</returns>
        public Container Container => Database.GetContainer(_options.ContainerName);

        private Database BuildDatabase()
        {
            var databaseResponse = Client.CreateDatabaseIfNotExistsAsync(_options.DatabaseName).ConfigureAwait(false).GetAwaiter().GetResult();
            databaseResponse.Database.CreateContainerIfNotExistsAsync(
                _options.ContainerName,
                _options.PartitionKeyPath,
                throughput: _options.Throughput)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            return databaseResponse.Database;
        }
    }
}
