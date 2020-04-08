using System;

namespace Pantry.Exceptions
{
    /// <summary>
    /// Base exception class.
    /// </summary>
    public class PantryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PantryException"/> class.
        /// </summary>
        public PantryException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PantryException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PantryException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PantryException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public PantryException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
