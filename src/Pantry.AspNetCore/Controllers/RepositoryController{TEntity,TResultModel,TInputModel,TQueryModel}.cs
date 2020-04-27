using Pantry.Queries;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Base implementation for controllers that can use repositories in a RESTful fashion.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResultModel">The model that is returned on Create, GetById, Update and Find.</typeparam>
    /// <typeparam name="TInputModel">The model that is used on Create and Update bodies.</typeparam>
    /// <typeparam name="TQueryModel">The model that is used for query parameters.</typeparam>
    public abstract class RepositoryController<TEntity, TResultModel, TInputModel, TQueryModel> : RepositoryController<TEntity, TResultModel, TResultModel, TInputModel, TInputModel, TQueryModel>
        where TEntity : class, IIdentifiable
        where TResultModel : class, IIdentifiable
        where TInputModel : class
        where TQueryModel : IRepositoryQuery<TEntity>, new()
    {
    }
}
