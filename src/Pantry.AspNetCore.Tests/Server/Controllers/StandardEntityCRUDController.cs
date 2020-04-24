using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/standard-entities-crud")]
    [ExposeCapabilities(Capabilities.CRUD)]
    public class StandardEntityCRUDController : RepositoryController<StandardEntity, StandardEntityCreateModel>
    {
    }
}
