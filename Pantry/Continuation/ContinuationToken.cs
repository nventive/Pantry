﻿using System;
using System.Text;
using System.Text.Json;
using Pantry.Exceptions;

namespace Pantry.Continuation
{
    /// <summary>
    /// Helper class to serialize/deserialize continuation tokens.
    /// </summary>
    public static class ContinuationToken
    {
        /// <summary>
        /// Encodes <paramref name="value"/> into a continuation token.
        /// Safe to transmit over HTTP/URI.
        /// </summary>
        /// <param name="value">The value to encode, if any.</param>
        /// <returns>The encoded continuation token, if any.</returns>
        public static string? ToContinuationToken(object? value)
        {
            if (value is null)
            {
                return null;
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)));
        }

        /// <summary>
        /// Recompose the <paramref name="continuationToken"/> into a <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The continuation token decoded type.</typeparam>
        /// <param name="continuationToken">The encoded continuation token.</param>
        /// <returns>The decoded continuation token, or default if no continuation token,.</returns>
        /// <exception cref="BadInputException">When the token is malformed.</exception>
        public static T? FromContinuationToken<T>(string? continuationToken)
            where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(continuationToken))
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(Convert.FromBase64String(continuationToken)));
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is JsonException)
                {
                    throw new BadInputException($"Malformed continuation token {continuationToken}", ex);
                }

                throw;
            }
        }
    }
}
