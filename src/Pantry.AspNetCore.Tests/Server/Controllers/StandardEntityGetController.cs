﻿using Microsoft.AspNetCore.Mvc;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Tests.Server.Controllers
{
    [Route("api/standard-entities-get")]
    [ExposeCapabilities(Capabilities.GetById)]
    public class StandardEntityGetController : RepositoryController<StandardEntity, StandardEntityCreateModel>
    {
    }
}
