using Pantry.DependencyInjection;

namespace Pantry.Dapper.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder"/> for Dapper.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IDapperRepositoryBuilder<TEntity> : IRepositoryBuilder
        where TEntity : class, IIdentifiable
    {
    }
}
