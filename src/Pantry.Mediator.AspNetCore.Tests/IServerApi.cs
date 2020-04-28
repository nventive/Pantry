using System.Threading.Tasks;
using Pantry.AspNetCore.Models;
using Pantry.Mediator.AspNetCore.Tests.Server;
using Refit;

namespace Pantry.Mediator.AspNetCore.Tests
{
    public interface IServerApi
    {
        [Get("/api/v1/standard-entities")]
        Task<ApiResponse<ContinuationEnumerableModel<StandardEntity>>> FindStandardEntities(FindStandardEntityQuery query);

        [Get("/api/v1/standard-entities/{id}")]
        Task<ApiResponse<StandardEntity>> GetStandardEntityById(string id);
    }
}
