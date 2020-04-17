using Pantry.Queries;

namespace Pantry.Azure.Cosmos.Queries
{
    /// <summary>
    /// Base class for CosmosDb Queries using Sql builder.
    /// </summary>
    /// <typeparam name="TResult">The query result.</typeparam>
    public abstract class CosmosSqlBuilderQuery<TResult> : Query<TResult>
    {
        /// <summary>
        /// Gets the SqlKata <see cref="SqlKata.Query"/>.
        /// !! Do not forget to add a WHERE clause for <see cref="CosmosDocument.TypeAttribute"/>
        /// if you want to only get the good entity type.
        /// </summary>
        /// <returns>The query to execute.</returns>
        public abstract SqlKata.Query GetQuery();
    }
}
