using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pantry.Mediator.AspNetCore.Execution
{
    /// <summary>
    /// Binds <see cref="HttpContext"/> to a <see cref="IDomainRequest"/> and launches
    /// execution through the <see cref="IMediator"/>.
    /// </summary>
    public interface IDomainRequestExecutor
    {
        /// <summary>
        /// Tries to execute the <paramref name="domainRequestType"/> from the <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="domainRequestType">The <see cref="IDomainRequest"/> type to execute.</param>
        /// <param name="options">The eventual <see cref="DomainRequestExecutionOptions"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ExecuteAsync(HttpContext context, Type domainRequestType, DomainRequestExecutionOptions? options);
    }
}
