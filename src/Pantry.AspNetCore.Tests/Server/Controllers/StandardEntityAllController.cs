using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/standard-entities-all")]
    public class StandardEntityAllController : RepositoryController<StandardEntity, StandardEntityModel, SummaryStandardEntityModel, StandardEntityCreateModel, StandardEntityUpdateModel, StandardEntityCriteriaQuery>
    {
    }
}
