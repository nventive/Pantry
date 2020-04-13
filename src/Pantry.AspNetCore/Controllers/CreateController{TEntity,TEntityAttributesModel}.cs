using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// A controller than only has a Create method.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityAttributesModel">The model that contains the attributes.</typeparam>
    public abstract class CreateController<TEntity, TEntityAttributesModel> : PantryController<TEntity, TEntityAttributesModel>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateController{TEntity, TEntityAttributesModel}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public CreateController(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>
        /// Create a <typeparamref name="TEntity"/> from <typeparamref name="TEntityAttributesModel"/>.
        /// </summary>
        /// <param name="model">The body.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TEntity>> Create([FromBody] TEntityAttributesModel model)
            => Ok(await CreateAction(model).ConfigureAwait(false));
    }
}
