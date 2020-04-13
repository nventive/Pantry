using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// A controller than has a GetById method.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public abstract class GetController<TEntity> : PantryController<TEntity, TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetController{TEntity}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public GetController(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>
        /// Get a <typeparamref name="TEntity"/> by its id.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TEntity>> GetById([FromRoute] string id)
            => OkNotFoundOrNotModified(await GetAction(id).ConfigureAwait(false));
    }
}
