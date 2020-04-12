using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Omu.ValueInjecter;
using Pantry.Mapping;
using Pantry.Traits;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// A controller than only has a create method.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityAttributesModel">The model that contains the attributes.</typeparam>
    public abstract class CreateController<TEntity, TEntityAttributesModel> : PantryController<TEntity>
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
        /// Creates a <typeparamref name="TEntity"/> from <typeparamref name="TEntityAttributesModel"/>.
        /// </summary>
        /// <param name="model">The body.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        [HttpPost]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<TEntity>> Create([FromBody] TEntityAttributesModel model)
        {
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

            return Ok(result);
        }
    }
}
