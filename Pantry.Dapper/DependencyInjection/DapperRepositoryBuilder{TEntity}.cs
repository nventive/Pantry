using Microsoft.Extensions.DependencyInjection;
using Pantry.DependencyInjection;

namespace Pantry.Dapper.DependencyInjection
{
    /// <summary>
    /// <see cref="IDapperRepositoryBuilder{TEntity}"/> default implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class DapperRepositoryBuilder<TEntity> : RepositoryBuilder, IDapperRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DapperRepositoryBuilder{TEntity}"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        public DapperRepositoryBuilder(IServiceCollection services)
            : base(services, typeof(TEntity))
        {
        }
    }
}
