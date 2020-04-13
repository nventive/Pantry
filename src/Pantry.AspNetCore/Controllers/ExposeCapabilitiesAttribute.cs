using System;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Needs to be placed on a controller deriving from <see cref="RepositoryController{TEntity, TEntityAttributesModel}"/>
    /// to tell the capabilities exposed by the base implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExposeCapabilitiesAttribute : Attribute, ICapabilitiesProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExposeCapabilitiesAttribute"/> class.
        /// </summary>
        /// <param name="capabilities">The capabilities to check.</param>
        public ExposeCapabilitiesAttribute(Capabilities capabilities)
        {
            Capabilities = capabilities;
        }

        /// <summary>
        /// Gets the <see cref="Capabilities"/>.
        /// </summary>
        public Capabilities Capabilities { get; }
    }
}
