using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Omu.ValueInjecter;
using Pantry.AspNetCore.ApplicationModels;
using Pantry.AspNetCore.Filters;
using Pantry.AspNetCore.Models;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Mapping;
using Pantry.Queries;
using Pantry.Traits;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Base implementation for controllers that can use repositories in a RESTful fashion.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityAttributesModel">The model that contains the attributes.</typeparam>
    [ApiController]
    public abstract class RepositoryController<TEntity, TEntityAttributesModel> : ControllerBase, ICapabilitiesProvider
        where TEntity : class, IIdentifiable
        where TEntityAttributesModel : class
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
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
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
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
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
        /// Find all <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="query">The query parameters.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [CapabilitiesApiExplorerVisibility(Capabilities.FindAll)]
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<ContinuationEnumerableModel<TEntity>>> FindAll(
            [FromQuery] FindAllQuery<TEntity> query)
        {
            if (!Capabilities.HasFlag(Capabilities.FindAll))
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            query ??= new FindAllQuery<TEntity>();

            var repository = ServiceProvider.GetRequiredService<IRepositoryFindAll<TEntity>>();

            return Ok(
                new ContinuationEnumerableModel<TEntity>(
                    await repository.FindAllAsync(query.ContinuationToken, query.Limit).ConfigureAwait(false)));
        }

        /// <summary>
        /// Update a <typeparamref name="TEntity"/> from <typeparamref name="TEntityAttributesModel"/>.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="model">The body.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [CapabilitiesApiExplorerVisibility(Capabilities.Update)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TEntity>> Update(
            [FromRoute] string id,
            [FromBody] TEntityAttributesModel model)
        {
            if (!Capabilities.HasFlag(Capabilities.Update))
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            var repository = ServiceProvider.GetRequiredService<IRepositoryUpdate<TEntity>>();
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

            entity.Id = id;
            if (entity is IETaggable taggable
                && Request.GetTypedHeaders().IfMatch.Any())
            {
                taggable.ETag = Request.GetTypedHeaders().IfMatch.First().Tag.Value?.Trim('"');
            }

            try
            {
                var result = await repository.UpdateAsync(entity).ConfigureAwait(false);
                return Ok(result);
            }
            catch (NotFoundException)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"{HttpContext.Request.Path} cannot be found.");
            }
            catch (ConcurrencyException)
            {
                return Problem(
                    statusCode: StatusCodes.Status412PreconditionFailed,
                    detail: $"Conflict error. Send with a more up-to-date ETag or no ETag to force the update.");
            }
        }

        /*
         * The following is commented out because the current support for JSON patch
         * needs Newtonsoft.Json serialization to work.
         * We will revisit this when this is no longer a requirement.
        /// <summary>
        /// Partially update a <typeparamref name="TEntity"/> using JSON patch.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="patch">The body.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [CapabilitiesApiExplorerVisibility(Capabilities.UpdatePartial)]
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TEntity>> UpdatePartial(
            [FromRoute] string id,
            [FromBody] JsonPatchDocument<TEntityAttributesModel> patch)
        {
            if (!Capabilities.HasFlag(Capabilities.UpdatePartial))
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            if (patch is null)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            var repositoryGet = ServiceProvider.GetRequiredService<IRepositoryGet<TEntity>>();
            var mapper = ServiceProvider.GetService<IMapper<TEntityAttributesModel, TEntity>>();

            var currentEntity = await repositoryGet.TryGetByIdAsync(id).ConfigureAwait(false);
            if (currentEntity is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"{HttpContext.Request.Path} cannot be found.");
            }

            TEntityAttributesModel currentAttributes;

            if (mapper != null)
            {
                currentAttributes = mapper.MapToSource(currentEntity);
            }
            else
            {
                currentAttributes = Mapper.Map<TEntity, TEntityAttributesModel>(currentEntity);
            }

            patch.ApplyTo(currentAttributes);

            return await Update(id, currentAttributes).ConfigureAwait(false);
        }*/

        /// <summary>
        /// Delete a <typeparamref name="TEntity"/> by its id.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        [CapabilitiesApiExplorerVisibility(Capabilities.Delete)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public virtual async Task<IActionResult> Delete([FromRoute] string id)
        {
            if (!Capabilities.HasFlag(Capabilities.Delete))
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            var repository = ServiceProvider.GetRequiredService<IRepositoryRemove<TEntity>>();

            var result = await repository.TryRemoveAsync(id).ConfigureAwait(false);

            if (result)
            {
                return NoContent();
            }
            else
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"{HttpContext.Request.Path} cannot be found.");
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
