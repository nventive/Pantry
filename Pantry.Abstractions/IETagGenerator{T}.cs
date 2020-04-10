using System.Threading.Tasks;

namespace Pantry
{
    /// <summary>
    /// Generates ETags.
    /// </summary>
    /// <typeparam name="T">The type to generate etags for.</typeparam>
    public interface IETagGenerator<T>
    {
        /// <summary>
        /// Generates an etag based on <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to generate an etag for.</param>
        /// <returns>The generated etag.</returns>
        ValueTask<string> Generate(T value);
    }
}
