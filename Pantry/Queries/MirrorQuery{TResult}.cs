namespace Pantry.Queries
{
    /// <summary>
    /// Specify criterias by mirroring properties.
    /// </summary>
    /// <typeparam name="TResult">The result element types.</typeparam>
    public class MirrorQuery<TResult> : Query<TResult>
        where TResult : class, new()
    {
        /// <summary>
        /// Gets or sets the Mirror specification.
        /// </summary>
        public TResult Mirror { get; set; } = new TResult();
    }
}
