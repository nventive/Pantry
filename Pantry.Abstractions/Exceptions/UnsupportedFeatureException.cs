using System;

namespace Pantry.Exceptions
{
    /// <summary>
    /// The feature is not supported by the implementation.
    /// </summary>
    public class UnsupportedFeatureException : PantryException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedFeatureException"/> class.
        /// </summary>
        public UnsupportedFeatureException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedFeatureException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UnsupportedFeatureException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedFeatureException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UnsupportedFeatureException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
