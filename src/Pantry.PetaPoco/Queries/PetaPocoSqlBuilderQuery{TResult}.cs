using Pantry.Queries;

namespace Pantry.PetaPoco.Queries
{
    /// <summary>
    /// Base class for CosmosDb Queries using Sql builder.
    /// </summary>
    /// <typeparam name="TResult">The query result.</typeparam>
    public abstract class PetaPocoSqlBuilderQuery<TResult> : RepositoryQuery<TResult>
    {
        /// <summary>
        /// Applies the query.
        /// </summary>
        /// <param name="sqlQuery">Customize the querying from there.</param>
        public abstract void Apply(SqlKata.Query sqlQuery);
    }
}
