using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Omu.ValueInjecter;
using Pantry.AspNetCore.ApplicationModels;
using Pantry.AspNetCore.Mapping;
using Pantry.AspNetCore.Models;
using Pantry.Continuation;
using Pantry.Exceptions;
using Pantry.Queries;
using Pantry.Traits;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Base implementation for controllers that can use repositories in a RESTful fashion.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResultModel">The model that is returned on Create, GetById and Update.</typeparam>
    /// <typeparam name="TResultsModel">The model that is returned on Find.</typeparam>
    /// <typeparam name="TCreateModel">The model that is used on Create body.</typeparam>
    /// <typeparam name="TUpdateModel">The model that is used on Update body.</typeparam>
    /// <typeparam name="TQueryModel">The model that is used for query parameters.</typeparam>
    [ApiController]
    public abstract class RepositoryController<TEntity, TResultModel, TResultsModel, TCreateModel, TUpdateModel, TQueryModel> : ControllerBase, ICapabilitiesProvider
        where TEntity : class, IIdentifiable
        where TResultModel : class, IIdentifiable
        where TResultsModel : class, IIdentifiable
        where TCreateModel : class
        where TUpdateModel : class
        where TQueryModel : IQuery<TEntity>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryController{TEntity, TResultModel, TResultsModel, TCreateModel, TUpdateModel, TQueryModel}"/> class.
        /// </summary>
        protected RepositoryController()
        {
            Capabilities = GetCapabilities();
        }

        /// <summary>
        /// Gets the capabilities.
        /// </summary>
        public Capabilities Capabilities { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        protected IServiceProvider Services => HttpContext.RequestServices;

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger => HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

        /// <summary>
        /// Create a <typeparamref name="TEntity"/> from <typeparamref name="TCreateModel"/>
        /// and returns a <typeparamref name="TResultModel"/>.
        /// </summary>
        /// <param name="model">The body.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [CapabilitiesApiExplorerVisibility(Capabilities.Create)]
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TResultModel>> Create([FromBody] TCreateModel model)
        {
            if (!Capabilities.HasFlag(Capabilities.Create))
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            var repository = Services.GetRequiredService<IRepositoryAdd<TEntity>>();
            var mapper = Services.GetService<IApiModelMapper<TCreateModel, TEntity>>();

            TEntity entity;
            if (mapper != null)
            {
                entity = await mapper.Map(model);
            }
            else
            {
                entity = Mapper.Map<TCreateModel, TEntity>(model);
            }

            var entityResult = await repository.AddAsync(entity).ConfigureAwait(false);
            var result = await MapSingleResult(entityResult).ConfigureAwait(false);

            if (!Capabilities.HasFlag(Capabilities.GetById))
            {
                return Ok(result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Id, version = HttpContext.Request.RouteValues["version"] }, result);
        }

        /// <summary>
        /// Find <typeparamref name="TResultsModel"/>.
        /// </summary>
        /// <param name="query">The query parameters.</param>
        /// <returns>The mapped results..</returns>
        [CapabilitiesApiExplorerVisibility(Capabilities.Find)]
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<ContinuationEnumerableModel<TResultsModel>>> Find(
            [FromQuery] TQueryModel query)
        {
            query ??= new TQueryModel();

            IContinuationEnumerable<TEntity> entityResults;
            if (query is ICriteriaQuery<TEntity> critQuery)
            {
                var repository = Services.GetRequiredService<IRepositoryFindByCriteria<TEntity>>();
                entityResults = await repository.FindAsync(critQuery).ConfigureAwait(false);
            }
            else
            {
                var repository = Services.GetRequiredService<IRepositoryFind<TEntity, TEntity, TQueryModel>>();
                entityResults = await repository.FindAsync(query).ConfigureAwait(false);
            }

            var results = await MapMultipleResults(entityResults).ConfigureAwait(false);

            return Ok(
                new ContinuationEnumerableModel<TResultsModel>(results, entityResults.ContinuationToken));
        }

        /// <summary>
        /// Get a <typeparamref name="TResultModel"/> by its id.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [CapabilitiesApiExplorerVisibility(Capabilities.GetById)]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TResultModel>> GetById([FromRoute] string id)
        {
            if (!Capabilities.HasFlag(Capabilities.GetById))
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            var repository = Services.GetRequiredService<IRepositoryGet<TEntity>>();

            var entityResult = await repository.TryGetByIdAsync(id).ConfigureAwait(false);

            if (entityResult is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"{HttpContext.Request.Path} cannot be found.");
            }
            else
            {
                var result = await MapSingleResult(entityResult).ConfigureAwait(false);
                return Ok(result);
            }
        }

        /// <summary>
        /// Update a <typeparamref name="TEntity"/> from <typeparamref name="TUpdateModel"/>
        /// and returns a <typeparamref name="TResultModel"/>.
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
        public virtual async Task<ActionResult<TResultModel>> Update(
            [FromRoute] string id,
            [FromBody] TUpdateModel model)
        {
            if (!Capabilities.HasFlag(Capabilities.Update))
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            var repository = Services.GetRequiredService<IRepositoryUpdate<TEntity>>();
            var mapper = Services.GetService<IApiModelMapper<TUpdateModel, TEntity>>();

            TEntity entity;
            if (mapper != null)
            {
                entity = await mapper.Map(model);
            }
            else
            {
                entity = Mapper.Map<TUpdateModel, TEntity>(model);
            }

            entity.Id = id;
            if (entity is IETaggable taggable
                && Request.GetTypedHeaders().IfMatch.Any())
            {
                taggable.ETag = Request.GetTypedHeaders().IfMatch.First().Tag.Value?.Trim('"');
            }

            try
            {
                var entityResult = await repository.UpdateAsync(entity).ConfigureAwait(false);
                var result = await MapSingleResult(entityResult).ConfigureAwait(false);
                return Ok(result);
            }
            catch (NotFoundException notFoundEx)
            {
                Logger.LogWarning(notFoundEx, "Error while trying to update {Model}: {Error}.", model, "NotFound");
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"{HttpContext.Request.Path} cannot be found.");
            }
            catch (ConcurrencyException concurrencyEx)
            {
                Logger.LogWarning(concurrencyEx, "Error while trying to update {Model}: {Error}.", model, "Concurrency");
                return Problem(
                    statusCode: StatusCodes.Status412PreconditionFailed,
                    detail: $"Conflict error. Send with a more up-to-date ETag or no ETag to force the update.");
            }
        }

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

            var repository = Services.GetRequiredService<IRepositoryRemove<TEntity>>();

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
        /// Maps a single <typeparamref name="TEntity"/> to <typeparamref name="TResultModel"/>.
        /// </summary>
        /// <param name="entity">The source entity.</param>
        /// <returns>The result model.</returns>
        protected virtual async ValueTask<TResultModel> MapSingleResult(TEntity entity)
        {
            if (typeof(TEntity) == typeof(TResultModel))
            {
                return (TResultModel)(object)entity;
            }

            var mapper = Services.GetService<IApiModelMapper<TEntity, TResultModel>>();

            if (mapper != null)
            {
                return await mapper.Map(entity);
            }

            return Mapper.Map<TEntity, TResultModel>(entity);
        }

        /// <summary>
        /// Maps multiple <typeparamref name="TEntity"/> to <typeparamref name="TResultModel"/>.
        /// </summary>
        /// <param name="entities">The source entities.</param>
        /// <returns>The result models.</returns>
        protected virtual async ValueTask<IEnumerable<TResultsModel>> MapMultipleResults(IEnumerable<TEntity> entities)
        {
            if (typeof(TEntity) == typeof(TResultsModel))
            {
                return entities.Select(x => (TResultsModel)(object)x);
            }

            var mapper = Services.GetService<IApiModelMapper<TEntity, TResultsModel>>();
            if (mapper != null)
            {
                return await mapper.Map(entities);
            }

            return entities.Select(x => Mapper.Map<TEntity, TResultsModel>(x));
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
            return exposeCapabilitiesAttribute is null ? Capabilities.All : exposeCapabilitiesAttribute.Capabilities;
        }
    }
}
