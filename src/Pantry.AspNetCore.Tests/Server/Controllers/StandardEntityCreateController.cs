using System;
using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/create-controller/entities")]
    public class StandardEntityCreateController : CreateController<StandardEntity, StandardEntityAttributes>
    {
        public StandardEntityCreateController(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
    }
}
