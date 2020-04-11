using System;

namespace Pantry.Exceptions
{
    /// <summary>
    /// An internal error occured. This is a bug.
    /// </summary>
    public class InternalErrorException : PantryException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalErrorException"/> class.
        /// </summary>
        public InternalErrorException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalErrorException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InternalErrorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalErrorException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InternalErrorException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
