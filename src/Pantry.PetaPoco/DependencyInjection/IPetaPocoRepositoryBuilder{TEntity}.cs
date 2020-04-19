using Pantry.DependencyInjection;

namespace Pantry.PetaPoco.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder{TEntity}"/> for PetaPoco.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IPetaPocoRepositoryBuilder<TEntity> : IRepositoryBuilder<TEntity>
        where TEntity : class, IIdentifiable
    {
    }
}
