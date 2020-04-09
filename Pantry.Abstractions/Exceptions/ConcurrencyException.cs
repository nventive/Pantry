using System;

namespace Pantry.Exceptions
{
    /// <summary>
    /// Represents an error when an concurrency occurs on updates.
    /// </summary>
    public class ConcurrencyException : PantryException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        public ConcurrencyException()
        {
            TargetType = string.Empty;
            TargetId = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public ConcurrencyException(string message)
            : base(message)
        {
            TargetType = string.Empty;
            TargetId = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {
            TargetType = string.Empty;
            TargetId = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="targetType">The type of target that was sought.</param>
        /// <param name="targetId">The id of target that was sought.</param>
        public ConcurrencyException(string targetType, string targetId)
            : base($"{targetType}/{targetId} has been updated concurrently before.")
        {
            TargetType = targetType;
            TargetId = targetId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="targetType">The type of target that was sought.</param>
        /// <param name="targetId">The id of target that was sought.</param>
        /// <param name="message">A message that describes the error.</param>
        public ConcurrencyException(string targetType, string targetId, string message)
            : base(message)
        {
            TargetType = targetType;
            TargetId = targetId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="targetType">The type of target that was sought.</param>
        /// <param name="targetId">The id of target that was sought.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConcurrencyException(string targetType, string targetId, Exception innerException)
            : base(innerException?.Message ?? $"{targetType}/{targetId} has been updated concurrently before.", innerException)
        {
            TargetType = targetType;
            TargetId = targetId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="targetType">The type of target that was sought.</param>
        /// <param name="targetId">The id of target that was sought.</param>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConcurrencyException(string targetType, string targetId, string message, Exception innerException)
            : base(message, innerException)
        {
            TargetType = targetType;
            TargetId = targetId;
        }

        /// <summary>
        /// Gets the type of target that was sought.
        /// </summary>
        public string TargetType { get; }

        /// <summary>
        /// Gets the id of target that was sought.
        /// </summary>
        public string TargetId { get; }
    }
}
