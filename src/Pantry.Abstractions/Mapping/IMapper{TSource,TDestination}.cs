namespace Pantry.Mapping
{
    /// <summary>
    /// Maps object between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    public interface IMapper<TSource, TDestination>
    {
        /// <summary>
        /// Maps <paramref name="source"/> to <typeparamref name="TDestination"/>.
        /// </summary>
        /// <param name="source">The source to map.</param>
        /// <returns>The destination mapped.</returns>
        TDestination MapToDestination(TSource source);

        /// <summary>
        /// Maps <paramref name="destination"/> to <typeparamref name="TSource"/>.
        /// </summary>
        /// <param name="destination">The destination to map.</param>
        /// <returns>The destination mapped.</returns>
        TSource MapToSource(TDestination destination);
    }
}
