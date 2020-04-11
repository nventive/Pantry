using System;

namespace Pantry.Exceptions
{
    /// <summary>
    /// A bad input data was received and rejected.
    /// </summary>
    public class BadInputException : PantryException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadInputException"/> class.
        /// </summary>
        public BadInputException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadInputException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BadInputException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadInputException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public BadInputException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
