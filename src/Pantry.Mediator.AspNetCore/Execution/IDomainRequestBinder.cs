using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pantry.Mediator.AspNetCore.Execution
{
    /// <summary>
    /// Responsible for binding HTTP request data to a domain request.
    /// </summary>
    public interface IDomainRequestBinder
    {
        /// <summary>
        /// Binds the HTTP request data in <paramref name="context"/> to a <paramref name="domainRequestType"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="domainRequestType">The <see cref="IDomainRequest"/> concrete type.</param>
        /// <returns>The <see cref="IDomainRequest"/> binded.</returns>
        ValueTask<IDomainRequest> Bind(HttpContext context, Type domainRequestType);
    }
}
