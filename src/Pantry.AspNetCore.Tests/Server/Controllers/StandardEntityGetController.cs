using System;
using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/get-controller/entities")]
    public class StandardEntityGetController : GetController<StandardEntity>
    {
        public StandardEntityGetController(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
    }
}
