using System;
using Microsoft.Extensions.DependencyInjection;
using Pantry.DependencyInjection;

namespace Pantry.Dapper.DependencyInjection
{
    /// <summary>
    /// <see cref="IDapperRepositoryBuilder"/> default implementation.
    /// </summary>
    public class DapperRepositoryBuilder : RepositoryBuilder, IDapperRepositoryBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DapperRepositoryBuilder"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="entityType">The entity type.</param>
        public DapperRepositoryBuilder(IServiceCollection services, Type entityType)
            : base(services, entityType)
        {
        }
    }
}
