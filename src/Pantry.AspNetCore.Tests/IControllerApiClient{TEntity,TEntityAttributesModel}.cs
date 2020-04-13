using System.Threading.Tasks;
using Refit;

namespace Pantry.AspNetCore.Tests
{
#nullable disable
    public interface IControllerApiClient<TEntity, TEntityAttributesModel>
    {
        [Post("")]
        Task<ApiResponse<TEntity>> Create([Body] TEntityAttributesModel attributes);

        [Get("/{id}")]
        Task<ApiResponse<TEntity>> GetById(
            string id,
            [Header("If-Modified-Since")] string ifModifiedSince = null,
            [Header("If-None-Match")] string ifNoneMatch = null);
    }
}
