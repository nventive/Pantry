using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using SqlKata.Compilers;

namespace Pantry.Azure.Cosmos
{
    /// <summary>
    /// SqlKata <see cref="Compiler"/> for CosmosDb.
    /// Support is EXTREMELY minimal. Handle with care.
    /// </summary>
    internal class CosmosDbQueryCompiler : Compiler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbQueryCompiler"/> class.
        /// </summary>
        public CosmosDbQueryCompiler()
        {
        }

        /// <inheritdoc/>
        public override string WrapIdentifiers(string input) => input;

        /// <inheritdoc/>
        public override string Wrap(string value) => value;

        /// <inheritdoc/>
        public override string WrapValue(string value) => value;

        /// <inheritdoc/>
        public override List<string> WrapArray(List<string> values) => values;

        /// <summary>
        /// Returns a CosmosDb <see cref="QueryDefinition"/>.
        /// </summary>
        /// <param name="query">The <see cref="SqlKata.Query"/>.</param>
        /// <returns>The CosmosDb query definition.</returns>
        public QueryDefinition ToQueryDefinition(SqlKata.Query query)
        {
            var sqlQuery = Compile(query);

            var definition = new QueryDefinition(sqlQuery.Sql);

            foreach (var binding in sqlQuery.NamedBindings)
            {
                definition = definition.WithParameter(binding.Key, binding.Value);
            }

            return definition;
        }
    }
}
