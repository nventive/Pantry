using System.ComponentModel.DataAnnotations;

namespace Pantry.Azure.Cosmos.Configuration
{
    /// <summary>
    /// Options for CosmosDb Repositories.
    /// </summary>
    public class CosmosRepositoryOptions
    {
        /// <summary>
        /// Gets the default connection string name.
        /// </summary>
        public const string DefaultConnectionStringName = "CosmosDb";

        /// <summary>
        /// Gets the default database name.
        /// </summary>
        public const string DefaultDatabaseName = "data";

        /// <summary>
        /// Gets the default container name.
        /// </summary>
        public const string DefaultContainerName = "data";

        /// <summary>
        /// Gets the default partition key path.
        /// </summary>
        public const string DefaultPartitionKeyPath = "/id";

        /// <summary>
        /// Gets or sets the Connection string.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Connection string name.
        /// </summary>
        public string? ConnectionStringName { get; set; } = DefaultConnectionStringName;

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        [Required]
        [StringLength(255)]
        public string DatabaseName { get; set; } = DefaultDatabaseName;

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        [Required]
        [StringLength(255)]
        public string ContainerName { get; set; } = DefaultContainerName;

        /// <summary>
        /// Gets or sets the PartitionKey path.
        /// </summary>
        [Required]
        public string PartitionKeyPath { get; set; } = DefaultPartitionKeyPath;

        /// <summary>
        /// Gets or sets the configured throughput used to create the container.
        /// </summary>
        public int? Throughput { get; set; }
    }
}
