using System.Threading.Tasks;
using Pantry.Exceptions;

namespace Pantry.Continuation
{
    /// <summary>
    /// Can help encode/decode continuation tokens.
    /// </summary>
    /// <typeparam name="TContinuationToken">The type of continuation token.</typeparam>
    public interface IContinuationTokenEncoder<TContinuationToken>
        where TContinuationToken : class
    {
        /// <summary>
        /// Encodes the <paramref name="continuationToken"/> into a continuation token.
        /// Safe to transmit over HTTP/URI.
        /// </summary>
        /// <param name="continuationToken">The <typeparamref name="TContinuationToken"/> to encode, if any.</param>
        /// <returns>The encoded continuation token, if any.</returns>
        ValueTask<string?> Encode(TContinuationToken? continuationToken);

        /// <summary>
        /// Recompose the <paramref name="value"/> into a <typeparamref name="TContinuationToken"/>.
        /// </summary>
        /// <param name="value">The encoded continuation token.</param>
        /// <returns>The decoded continuation token, or default if no continuation token,.</returns>
        /// <exception cref="BadInputException">When the <paramref name="value"/> is malformed.</exception>
        ValueTask<TContinuationToken?> Decode(string? value);
    }
}
