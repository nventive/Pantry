using Microsoft.Azure.Cosmos;
using Pantry.Queries;

namespace Pantry.Azure.Cosmos.Queries
{
    /// <summary>
    /// Base class for CosmosDb Queries using Sql.
    /// </summary>
    /// <typeparam name="TResult">The query result.</typeparam>
    public abstract class CosmosSqlQuery<TResult> : Query<TResult>
    {
        /// <summary>
        /// Gets the CosmosDb <see cref="QueryDefinition"/>.
        /// !! Do not forget to add a WHERE clause for <see cref="CosmosDocument.TypeAttribute"/>
        /// if you want to only get the good entity type.
        /// </summary>
        /// <returns>The query to execute.</returns>
        public abstract QueryDefinition GetQueryDefinition();
    }
}
