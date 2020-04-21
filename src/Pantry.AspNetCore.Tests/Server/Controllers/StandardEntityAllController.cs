using System;
using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/standard-entities-all")]
    [ExposeCapabilities(Capabilities.All)]
    public class StandardEntityAllController : RepositoryController<StandardEntity, StandardEntityAttributes>
    {
        public StandardEntityAllController(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
    }
}
