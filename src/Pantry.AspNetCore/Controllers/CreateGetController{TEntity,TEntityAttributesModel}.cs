using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// A controller than has a Create and a Get method.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityAttributesModel">The model that contains the attributes.</typeparam>
    public abstract class CreateGetController<TEntity, TEntityAttributesModel> : PantryController<TEntity, TEntityAttributesModel>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateGetController{TEntity, TEntityAttributesModel}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public CreateGetController(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>
        /// Create a <typeparamref name="TEntity"/> from <typeparamref name="TEntityAttributesModel"/>.
        /// </summary>
        /// <param name="model">The body.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TEntity>> Create([FromBody] TEntityAttributesModel model)
        {
            var result = await CreateAction(model).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = result.Id, version = HttpContext.Request.RouteValues["version"] }, result);
        }

        /// <summary>
        /// Get a <typeparamref name="TEntity"/> by its id.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TEntity>> GetById([FromRoute] string id)
            => OkOrNotFound(await GetAction(id).ConfigureAwait(false));
    }
}
