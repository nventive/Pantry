using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Pantry.Exceptions;

namespace Pantry.Continuation
{
    /// <summary>
    /// <see cref="IContinuationTokenEncoder{TContinuationToken}"/> implementation that Base64 + JSON encode
    /// the tokens.
    /// </summary>
    /// <typeparam name="TContinuationToken">The type of continuation token.</typeparam>
    public class Base64JsonContinuationTokenEncoder<TContinuationToken> : IContinuationTokenEncoder<TContinuationToken>
        where TContinuationToken : class
    {
        /// <inheritdoc/>
        public ValueTask<string?> Encode(TContinuationToken? continuationToken)
        {
            if (continuationToken is null)
            {
                return new ValueTask<string?>((string?)null);
            }

            return new ValueTask<string?>(
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        JsonSerializer.Serialize(continuationToken))));
        }

        /// <inheritdoc/>
        public ValueTask<TContinuationToken?> Decode(string? value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return new ValueTask<TContinuationToken?>(default(TContinuationToken?));
                }

                return new ValueTask<TContinuationToken?>(
                    JsonSerializer.Deserialize<TContinuationToken>(
                        Encoding.UTF8.GetString(
                            Convert.FromBase64String(value))));
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is JsonException)
                {
                    throw new BadInputException($"Malformed continuation token {value}", ex);
                }

                throw;
            }
        }
    }
}
