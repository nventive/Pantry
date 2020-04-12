using System;
using Microsoft.AspNetCore.Components;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/create-controller/entities")]
    public class CreaterStandardEntityController : CreateController<StandardEntity, StandardEntityAttributes>
    {
        public CreaterStandardEntityController(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
    }
}
