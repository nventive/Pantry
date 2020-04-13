using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.ApplicationModels
{
    /// <summary>
    /// <see cref="IActionModelConvention"/> that toggles api explorer visibility
    /// based on <see cref="CapabilitiesApiExplorerVisibilityAttribute"/>.
    /// </summary>
    public class CapabilitiesActionModelConvention : IActionModelConvention
    {
        /// <inheritdoc/>
        public void Apply(ActionModel action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var capabilitiesApiExplorerVisibilityAttribute = action.Attributes.OfType<CapabilitiesApiExplorerVisibilityAttribute>().FirstOrDefault();
            if (capabilitiesApiExplorerVisibilityAttribute is null)
            {
                return;
            }

            var exposeCapabilitiesAttribute = action.Controller.Attributes.OfType<ExposeCapabilitiesAttribute>().FirstOrDefault();
            if (exposeCapabilitiesAttribute is null)
            {
                return;
            }

            if (!capabilitiesApiExplorerVisibilityAttribute.Capabilities.HasFlag(exposeCapabilitiesAttribute.Capabilities))
            {
                action.ApiExplorer.IsVisible = false;
            }
        }
    }
}
