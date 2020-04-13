using System;
using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/standard-entities")]
    [ExposeCapabilities(Capabilities.CRUD)]
    public class StandardEntityCRUDController : RepositoryController<StandardEntity, StandardEntityAttributes>
    {
        public StandardEntityCRUDController(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
    }
}
