using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Omu.ValueInjecter;
using Pantry.AspNetCore.ApplicationModels;
using Pantry.AspNetCore.Filters;
using Pantry.Mapping;
using Pantry.Traits;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Base implementation for controllers that can use repositories in a RESTful fashion.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityAttributesModel">The model that contains the attributes.</typeparam>
    [ApiController]
    [EntityHttpCacheResponseHeaders]
    public abstract class RepositoryController<TEntity, TEntityAttributesModel> : ControllerBase, ICapabilitiesProvider
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryController{TEntity, TEntityAttributesModel}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public RepositoryController(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Capabilities = GetCapabilities();
        }

        /// <summary>
        /// Gets the capabilities.
        /// </summary>
        public Capabilities Capabilities { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Create a <typeparamref name="TEntity"/> from <typeparamref name="TEntityAttributesModel"/>.
        /// </summary>
        /// <param name="model">The body.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [CapabilitiesApiExplorerVisibility(Capabilities.Create)]
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TEntity>> Create([FromBody] TEntityAttributesModel model)
        {
            if (!Capabilities.HasFlag(Capabilities.Create))
            {
                return NotFound();
            }

            var repository = ServiceProvider.GetRequiredService<IRepositoryAdd<TEntity>>();
            var mapper = ServiceProvider.GetService<IMapper<TEntityAttributesModel, TEntity>>();

            TEntity entity;
            if (mapper != null)
            {
                entity = mapper.MapToDestination(model);
            }
            else
            {
                entity = Mapper.Map<TEntityAttributesModel, TEntity>(model);
            }

            var result = await repository.AddAsync(entity).ConfigureAwait(false);

            if (!Capabilities.HasFlag(Capabilities.GetById))
            {
                return Ok(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Id, version = HttpContext.Request.RouteValues["version"] }, result);
        }

        /// <summary>
        /// Get a <typeparamref name="TEntity"/> by its id.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [CapabilitiesApiExplorerVisibility(Capabilities.GetById)]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TEntity>> GetById([FromRoute] string id)
        {
            if (!Capabilities.HasFlag(Capabilities.GetById))
            {
                return NotFound();
            }

            var repository = ServiceProvider.GetRequiredService<IRepositoryGet<TEntity>>();

            var result = await repository.TryGetByIdAsync(id).ConfigureAwait(false);

            if (result is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"{HttpContext.Request.Path} cannot be found.");
            }
            else
            {
                return Ok(result);
            }
        }

        /// <summary>
        /// Gets the <see cref="Capabilities"/> of the current controller.
        /// </summary>
        /// <returns>The current <see cref="Capabilities"/>.</returns>
        private Capabilities GetCapabilities()
        {
            var exposeCapabilitiesAttribute = GetType().GetCustomAttributes(typeof(ExposeCapabilitiesAttribute), true)
                .Cast<ExposeCapabilitiesAttribute>()
                .FirstOrDefault();
            return exposeCapabilitiesAttribute is null ? Capabilities.CRUD : exposeCapabilitiesAttribute.Capabilities;
        }
    }
}
