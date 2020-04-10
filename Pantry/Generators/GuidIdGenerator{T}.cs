using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Pantry.Generators
{
    /// <summary>
    /// <see cref="IIdGenerator{T}"/> implementation using <see cref="Guid"/>.
    /// </summary>
    /// <typeparam name="T">The type to generate ids for.</typeparam>
    public class GuidIdGenerator<T> : IIdGenerator<T>
        where T : class
    {
        /// <inheritdoc/>
        public ValueTask<string> Generate(T? value) => new ValueTask<string>(Guid.NewGuid().ToString("n", CultureInfo.InvariantCulture));
    }
}
