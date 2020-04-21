using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Pantry.AspNetCore.Models;
using Pantry.Queries;
using Pantry.Traits;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Base implementation for controllers that executes queries.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityAttributesModel">The model that contains the attributes.</typeparam>
    /// <typeparam name="TCriteriaQuery">The query type.</typeparam>
    public abstract class RepositoryController<TEntity, TEntityAttributesModel, TCriteriaQuery> : RepositoryController<TEntity, TEntityAttributesModel>
        where TEntity : class, IIdentifiable
        where TEntityAttributesModel : class
        where TCriteriaQuery : ICriteriaQuery<TEntity>, new()
    {
        /// <summary>
        /// Find all <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="query">The query parameters.</param>
        /// <returns>An <see cref="ActionResult{TEntity}"/>.</returns>
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public virtual async Task<ActionResult<ContinuationEnumerableModel<TEntity>>> Find(
            [FromQuery] TCriteriaQuery query)
        {
            query ??= new TCriteriaQuery();

            var repository = Services.GetService<IRepositoryFindByCriteria<TEntity>>();

            return Ok(
                new ContinuationEnumerableModel<TEntity>(
                    await repository.FindAsync(query).ConfigureAwait(false)));
        }
    }
}
