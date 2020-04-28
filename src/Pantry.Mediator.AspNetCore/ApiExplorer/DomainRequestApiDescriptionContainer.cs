using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Pantry.Mediator.AspNetCore.ApiExplorer
{
    /// <summary>
    /// Holds registration information for <see cref="IDomainRequest"/> mapped endpoints.
    /// </summary>
    public class DomainRequestApiDescriptionContainer : List<ApiDescription>
    {
    }
}
