using System;
using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/create-get-controller/entities")]
    public class StandardEntityCreateGetController : CreateGetController<StandardEntity, StandardEntityAttributes>
    {
        public StandardEntityCreateGetController(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
    }
}
