using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Pantry
{
    /// <summary>
    /// Default Id Generator.
    /// </summary>
    /// <typeparam name="T">The type to generate ids for.</typeparam>
    public class IdGenerator<T> : IIdGenerator<T>
        where T : class
    {
        /// <inheritdoc/>
        public ValueTask<string> Generate(T? entity) => new ValueTask<string>(Guid.NewGuid().ToString("n", CultureInfo.InvariantCulture));
    }
}
