using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.ApplicationModels
{
    /// <summary>
    /// <see cref="IActionModelConvention"/> that disables actions
    /// based on <see cref="CapabilitiesApiExplorerVisibilityAttribute"/>.
    /// </summary>
    public class CapabilitiesApplicationModelConvention : IApplicationModelConvention
    {
        /// <inheritdoc/>
        public void Apply(ApplicationModel application)
        {
            if (application is null)
            {
                throw new System.ArgumentNullException(nameof(application));
            }

            foreach (var controller in application.Controllers.Where(x => typeof(ICapabilitiesProvider).IsAssignableFrom(x.ControllerType)))
            {
                var exposeCapabilitiesAttribute = controller.Attributes.OfType<ExposeCapabilitiesAttribute>().FirstOrDefault();
                var exposeCapabilities = exposeCapabilitiesAttribute?.Capabilities ?? Capabilities.All;

                var actionsToRemove = controller
                    .Actions
                    .Select(action => (action, cap: action.Attributes.OfType<CapabilitiesApiExplorerVisibilityAttribute>().FirstOrDefault()))
                    .Where(x => x.cap != null && (x.cap.Capabilities & exposeCapabilities) != x.cap.Capabilities)
                    .Select(x => x.action)
                    .ToList();

                foreach (var actionToRemove in actionsToRemove)
                {
                    controller.Actions.Remove(actionToRemove);
                }
            }
        }
    }
}
