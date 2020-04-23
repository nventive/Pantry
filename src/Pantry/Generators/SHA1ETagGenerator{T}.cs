using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pantry.Generators
{
    /// <summary>
    /// <see cref="IETagGenerator{T}"/> that uses SHA1.
    /// </summary>
    /// <typeparam name="T">The type to generate etags for.</typeparam>
    public class SHA1ETagGenerator<T> : IETagGenerator<T>
    {
        [SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "No crypto here - just hashing.")]
        private static readonly HashAlgorithm _hash = SHA1.Create();

        /// <inheritdoc/>
        public ValueTask<string> Generate(T value)
            => new ValueTask<string>(
                string.Concat(
                    _hash.ComputeHash(
                        Encoding.UTF8.GetBytes(
                            value is string valueStr ? valueStr : JsonSerializer.Serialize(value)))
                    .Select(
                        x => x.ToString(
                            "x2",
                            CultureInfo.InvariantCulture))));
    }
}
