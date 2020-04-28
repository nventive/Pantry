using System.Net.Http;
using System.Threading.Tasks;
using Pantry.AspNetCore.Models;
using Pantry.Mediator.AspNetCore.Tests.Server;
using Refit;

namespace Pantry.Mediator.AspNetCore.Tests
{
    public interface IServerApi
    {
        [Post("/api/v1/standard-entities")]
        Task<ApiResponse<StandardEntity>> CreateStandardEntity([Body] CreateStandardEntityCommand model);

        [Get("/api/v1/standard-entities")]
        Task<ApiResponse<ContinuationEnumerableModel<StandardEntity>>> FindStandardEntities(FindStandardEntityQuery query);

        [Get("/api/v1/standard-entities/{id}")]
        Task<ApiResponse<StandardEntity>> GetStandardEntityById(string id);

        [Put("/api/v1/standard-entities/{id}")]
        Task<ApiResponse<StandardEntity>> UpdateStandardEntity(string id, [Body] UpdateStandardEntityCommand model);

        [Delete("/api/v1/standard-entities/{id}")]
        Task<HttpResponseMessage> DeleteStandardEntity(string id);
    }
}
