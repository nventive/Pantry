using System;

namespace Pantry.Exceptions
{
    /// <summary>
    /// Represents an error when an entity conflicts with an existing one..
    /// </summary>
    public class ConflictException : PantryException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        public ConflictException()
        {
            TargetType = string.Empty;
            TargetId = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public ConflictException(string message)
            : base(message)
        {
            TargetType = string.Empty;
            TargetId = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
            TargetType = string.Empty;
            TargetId = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        /// <param name="targetType">The type of target that was sought.</param>
        /// <param name="targetId">The id of target that was sought.</param>
        public ConflictException(string targetType, string targetId)
            : base($"{targetType}/{targetId} not found.")
        {
            TargetType = targetType;
            TargetId = targetId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        /// <param name="targetType">The type of target that was sought.</param>
        /// <param name="targetId">The id of target that was sought.</param>
        /// <param name="message">A message that describes the error.</param>
        public ConflictException(string targetType, string targetId, string message)
            : base(message)
        {
            TargetType = targetType;
            TargetId = targetId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        /// <param name="targetType">The type of target that was sought.</param>
        /// <param name="targetId">The id of target that was sought.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConflictException(string targetType, string targetId, Exception innerException)
            : base(innerException?.Message ?? $"{targetType}/{targetId} not found.", innerException)
        {
            TargetType = targetType;
            TargetId = targetId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        /// <param name="targetType">The type of target that was sought.</param>
        /// <param name="targetId">The id of target that was sought.</param>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConflictException(string targetType, string targetId, string message, Exception innerException)
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
