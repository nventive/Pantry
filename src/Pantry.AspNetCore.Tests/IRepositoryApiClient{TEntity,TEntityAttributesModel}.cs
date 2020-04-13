using System.Net.Http;
using System.Threading.Tasks;
using Refit;

namespace Pantry.AspNetCore.Tests
{
#nullable disable
    public interface IRepositoryApiClient<TEntity, TEntityAttributesModel>
        where TEntityAttributesModel : class
    {
        [Post("")]
        Task<ApiResponse<TEntity>> Create([Body] TEntityAttributesModel attributes);

        [Get("/{id}")]
        Task<ApiResponse<TEntity>> GetById(
            string id,
            [Header("If-Modified-Since")] string ifModifiedSince = null,
            [Header("If-None-Match")] string ifNoneMatch = null);

        [Put("/{id}")]
        Task<ApiResponse<TEntity>> Update(
            string id,
            [Body] TEntityAttributesModel attributes,
            [Header("If-Match")] string ifMatch = null);

        [Delete("/{id}")]
        Task<HttpResponseMessage> Delete(string id);
    }
}
