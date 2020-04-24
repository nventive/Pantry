using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Pantry.AspNetCore.Models;

using Refit;

namespace Pantry.AspNetCore.Tests
{
#nullable disable
    public interface IRepositoryApiClient<TResultModel, TResultsModel, TCreateModel, TUpdateModel, TQueryModel>
    {
        [Post("")]
        Task<ApiResponse<TResultModel>> Create([Body] TCreateModel attributes);

        [Get("/{id}")]
        Task<ApiResponse<TResultModel>> GetById(
            string id,
            [Header("If-Modified-Since")] string ifModifiedSince = null,
            [Header("If-None-Match")] string ifNoneMatch = null);

        [Get("")]
        Task<ApiResponse<ContinuationEnumerableModel<TResultsModel>>> Find(TQueryModel model);

        [Put("/{id}")]
        Task<ApiResponse<TResultModel>> Update(
            string id,
            [Body] TUpdateModel attributes,
            [Header("If-Match")] string ifMatch = null);

        [Delete("/{id}")]
        Task<HttpResponseMessage> Delete(string id);
    }
}
