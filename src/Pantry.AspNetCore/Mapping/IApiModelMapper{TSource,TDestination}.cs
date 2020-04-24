using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// <returns>The destination.</returns>
        ValueTask<TDestination> Map(TSource source);

        /// <summary>
        /// Map sources to destinations.
        /// </summary>
        /// <param name="sources">The sources.</param>
        /// <returns>The destinations.</returns>
        ValueTask<IEnumerable<TDestination>> Map(IEnumerable<TSource> sources);
    }
}
