using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using SqlKata.Compilers;

namespace Pantry.Azure.Cosmos
{
    /// <summary>
    /// SqlKata <see cref="Compiler"/> for CosmosDb.
    /// Support is minimal. Handle with care.
    /// </summary>
    public class CosmosQueryCompiler : Compiler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosQueryCompiler"/> class.
        /// </summary>
        public CosmosQueryCompiler()
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
        public virtual QueryDefinition ToQueryDefinition(SqlKata.Query query)
        {
            var sqlQuery = Compile(query);

            var definition = new QueryDefinition(sqlQuery.Sql);

            foreach (var binding in sqlQuery.NamedBindings)
            {
                var parameterValue = binding.Value;
                if (parameterValue is DateTimeOffset dto)
                {
                    parameterValue = dto.ToUnixTimeMilliseconds();
                }

                definition = definition.WithParameter(binding.Key, parameterValue);
            }

            return definition;
        }
    }
}
