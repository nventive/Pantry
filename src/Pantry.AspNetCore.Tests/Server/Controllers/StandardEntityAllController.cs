using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/standard-entities-all")]
    [ExposeCapabilities(Capabilities.CRUD | Capabilities.FindAll)]
    public class StandardEntityAllController : RepositoryController<StandardEntity, StandardEntityAttributes>
    {
    }
}
