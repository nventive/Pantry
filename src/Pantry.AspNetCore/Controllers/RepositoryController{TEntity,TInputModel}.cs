using Pantry.Queries;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Base implementation for controllers that can use repositories in a RESTful fashion.
    /// </summary>
    /// <typeparam name="TEntity">The entity type and model returned on Create, GetById, Update and Find.</typeparam>
    /// <typeparam name="TInputModel">The model that is used on Create and Update bodies.</typeparam>
    public abstract class RepositoryController<TEntity, TInputModel> : RepositoryController<TEntity, TEntity, TEntity, TInputModel, TInputModel, CriteriaQuery<TEntity>>
        where TEntity : class, IIdentifiable
        where TInputModel : class
    {
    }
}
