using System;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.ApplicationModels
{
    /// <summary>
    /// Toggles Api Explorer visibility based on <see cref="Capabilities"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CapabilitiesApiExplorerVisibilityAttribute : Attribute, ICapabilitiesProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CapabilitiesApiExplorerVisibilityAttribute"/> class.
        /// </summary>
        /// <param name="capabilities">The capabilities to check.</param>
        public CapabilitiesApiExplorerVisibilityAttribute(Capabilities capabilities)
        {
            Capabilities = capabilities;
        }

        /// <summary>
        /// Gets the <see cref="Capabilities"/>.
        /// </summary>
        public Capabilities Capabilities { get; }
    }
}
