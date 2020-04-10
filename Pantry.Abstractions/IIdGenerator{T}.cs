using System.Threading.Tasks;

namespace Pantry
{
    /// <summary>
    /// Generates ids for <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to generate ids for.</typeparam>
    public interface IIdGenerator<T>
        where T : class
    {
        /// <summary>
        /// Generates a unique id.
        /// </summary>
        /// <param name="value">The value to generate an id for, eventually.</param>
        /// <returns>The generated id.</returns>
        ValueTask<string> Generate(T? value);
    }
}
