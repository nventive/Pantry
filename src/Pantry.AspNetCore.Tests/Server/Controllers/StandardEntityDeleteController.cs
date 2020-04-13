using System;
using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/standard-entities-delete")]
    [ExposeCapabilities(Capabilities.Delete)]
    public class StandardEntityDeleteController : RepositoryController<StandardEntity, StandardEntityAttributes>
    {
        public StandardEntityDeleteController(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
    }
}
