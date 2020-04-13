using System.Threading.Tasks;
using Refit;

namespace Pantry.AspNetCore.Tests
{
    public interface IControllerApiClient<TEntity, TEntityAttributesModel>
    {
        [Post("")]
        Task<ApiResponse<TEntity>> Create([Body] TEntityAttributesModel attributes);

        [Get("/{id}")]
        Task<ApiResponse<TEntity>> GetById(string id);
    }
}
