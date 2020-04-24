using Pantry.Queries;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Base implementation for controllers that can use repositories in a RESTful fashion.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResultModel">The model that is returned on Create, GetById, Update and Find.</typeparam>
    /// <typeparam name="TCreateModel">The model that is used on Create body.</typeparam>
    /// <typeparam name="TUpdateModel">The model that is used on Update body.</typeparam>
    /// <typeparam name="TQueryModel">The model that is used for query parameters.</typeparam>
    public abstract class RepositoryController<TEntity, TResultModel, TCreateModel, TUpdateModel, TQueryModel> : RepositoryController<TEntity, TResultModel, TResultModel, TCreateModel, TUpdateModel, TQueryModel>
        where TEntity : class, IIdentifiable
        where TResultModel : class, IIdentifiable
        where TCreateModel : class
        where TUpdateModel : class
        where TQueryModel : IQuery<TEntity>, new()
    {
    }
}
