namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Provides information about <see cref="Capabilities"/>.
    /// </summary>
    public interface ICapabilitiesProvider
    {
        /// <summary>
        /// Gets the <see cref="Capabilities"/>.
        /// </summary>
        Capabilities Capabilities { get; }
    }
}
