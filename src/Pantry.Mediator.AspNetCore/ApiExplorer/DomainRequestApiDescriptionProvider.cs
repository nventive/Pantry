using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Pantry.Mediator.AspNetCore.ApiExplorer
{
    /// <summary>
    /// <see cref="IApiDescriptionProvider"/> for <see cref="IDomainRequest"/> mapped endpoints.
    /// </summary>
    public class DomainRequestApiDescriptionProvider : IApiDescriptionProvider
    {
        private readonly DomainRequestApiDescriptionContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainRequestApiDescriptionProvider"/> class.
        /// </summary>
        /// <param name="container">The container for registered endpoints.</param>
        public DomainRequestApiDescriptionProvider(
            DomainRequestApiDescriptionContainer container)
        {
            _container = container ?? throw new System.ArgumentNullException(nameof(container));
        }

        /// <inheritdoc/>
        public int Order => 0;

        /// <inheritdoc/>
        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }

        /// <inheritdoc/>
        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var apiDescription in _container)
            {
                context.Results.Add(apiDescription);
            }
        }
    }
}
