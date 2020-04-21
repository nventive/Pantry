using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/standard-entities-query")]
    public class StandardEntityQueryController : RepositoryController<StandardEntity, StandardEntityAttributes, StandardEntityCriteriaQuery>
    {
    }
}
