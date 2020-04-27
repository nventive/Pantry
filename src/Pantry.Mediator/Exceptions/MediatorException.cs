using System;

namespace Pantry.Mediator.Exceptions
{
    /// <summary>
    /// Technnical exception when running the mediator.
    /// </summary>
    public class MediatorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediatorException"/> class.
        /// </summary>
        public MediatorException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediatorException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MediatorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediatorException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MediatorException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
