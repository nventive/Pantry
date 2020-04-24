using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pantry.AspNetCore.Mapping
{
    /// <summary>
    /// Mapper for API Models.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    public interface IApiModelMapper<TSource, TDestination>
    {
        /// <summary>
        /// Map a single source to a destination.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="context">The current <see cref="HttpContext"/>.</param>
        /// <returns>The destination.</returns>
        ValueTask<TDestination> Map(TSource source, HttpContext context);

        /// <summary>
        /// Map sources to destinations.
        /// </summary>
        /// <param name="sources">The sources.</param>
        /// <param name="context">The current <see cref="HttpContext"/>.</param>
        /// <returns>The destinations.</returns>
        ValueTask<IEnumerable<TDestination>> Map(IEnumerable<TSource> sources, HttpContext context);
    }
}
