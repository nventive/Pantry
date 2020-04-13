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
    /// Base class for REST controllers.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityAttributesModel">The model that contains the attributes.</typeparam>
    [ApiController]
    public abstract class PantryController<TEntity, TEntityAttributesModel> : ControllerBase
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PantryController{TEntity, TEntityAttributesModel}"/> class.
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

        /// <summary>
        /// Implements the Create action.
        /// </summary>
        /// <param name="model">The model attributes.</param>
        /// <returns>The created entity.</returns>
        protected virtual async Task<TEntity> CreateAction(TEntityAttributesModel model)
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

            return await repository.AddAsync(entity).ConfigureAwait(false);
        }

        /// <summary>
        /// Implements the Get action.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <returns>The entity, or null if not found.</returns>
        protected virtual async Task<TEntity?> GetAction(string id)
        {
            var repository = ServiceProvider.GetRequiredService<IRepositoryGet<TEntity>>();

            return await repository.TryGetByIdAsync(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Return either OK or NotFound depending on whether the <paramref name="result"/> is null.
        /// </summary>
        /// <typeparam name="TResult">The type of result.</typeparam>
        /// <param name="result">The result.</param>
        /// <returns><see cref="OkObjectResult"/> is not result is not null, or a <see cref="ObjectResult"/> with 404 and <see cref="ProblemDetails"/> if it is.</returns>
        protected virtual ActionResult<TResult> OkOrNotFound<TResult>(TResult? result)
            where TResult : class
        {
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
    }
}
