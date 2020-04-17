using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly CosmosRepositoryOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosContainerFactory"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        /// <param name="options">The <see cref="CosmosRepositoryOptions"/>.</param>
        public CosmosContainerFactory(
            IConfiguration configuration,
            IOptions<CosmosRepositoryOptions> options)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _lazyClient = new Lazy<CosmosClient>(BuildClient);
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

        private CosmosClient BuildClient()
        {
            var connectionString = _options.ConnectionString ?? _configuration.GetConnectionString(_options.ConnectionStringName);
            return new CosmosClient(connectionString);
        }

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
