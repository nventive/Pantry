using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/standard-entities-update")]
    [ExposeCapabilities(Capabilities.Update)]
    public class StandardEntityUpdateController : RepositoryController<StandardEntity, StandardEntityAttributes>
    {
    }
}
