using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/standard-entities-create")]
    [ExposeCapabilities(Capabilities.Create)]
    public class StandardEntityCreateController : RepositoryController<StandardEntity, StandardEntityAttributes>
    {
    }
}
