using Pantry.Queries;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Base implementation for controllers that can use repositories in a RESTful fashion.
    /// </summary>
    /// <typeparam name="TEntity">The entity type and model returned on Create, GetById, Update and Find.</typeparam>
    /// <typeparam name="TInputModel">The model that is used on Create and Update bodies.</typeparam>
    /// <typeparam name="TQueryModel">The model that is used for query parameters.</typeparam>
    public abstract class RepositoryController<TEntity, TInputModel, TQueryModel> : RepositoryController<TEntity, TEntity, TEntity, TInputModel, TInputModel, TQueryModel>
        where TEntity : class, IIdentifiable
        where TInputModel : class
        where TQueryModel : IRepositoryQuery<TEntity>, new()
    {
    }
}
