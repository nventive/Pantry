using System;
using Microsoft.AspNetCore.Mvc;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Base class for REST controllers.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    [ApiController]
    public abstract class PantryController<TEntity> : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PantryController{TEntity}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public PantryController(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }
    }
}
