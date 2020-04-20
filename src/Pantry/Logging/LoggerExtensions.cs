using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Pantry.Logging
{
    /// <summary>
    /// <see cref="ILogger"/> extension methods.
    /// </summary>
    public static class LoggerExtensions
    {
        private const int EventIdBase = 10000;
        private const int EventIdWarningBase = 11000;

        private static readonly Action<ILogger, string?, string?, string?, object?, Exception?> _logGetById =
            LoggerMessage.Define<string?, string?, string?, object?>(
                LogLevel.Trace,
                new EventId(EventIdBase + 2, "PantryGetById"),
                "{Method}() = {EntityType}.{EntityId} {Entity}");

        private static readonly Action<ILogger, string?, string?, string?, object?, Exception?> _logAdded =
            LoggerMessage.Define<string?, string?, string?, object?>(
                LogLevel.Debug,
                new EventId(EventIdBase + 3, "PantryAdded"),
                "{Method}() = {EntityType}.{EntityId} {Entity}");

        private static readonly Action<ILogger, string?, string?, string?, object?, string?, Exception?> _logAddedWarning =
            LoggerMessage.Define<string?, string?, string?, object?, string?>(
                LogLevel.Warning,
                new EventId(EventIdWarningBase + 3, "PantryAddedWarning"),
                "{Method}() = {EntityType}.{EntityId} {Entity} {Warning}");

        private static readonly Action<ILogger, string?, string?, string?, object?, Exception?> _logUpdated =
            LoggerMessage.Define<string?, string?, string?, object?>(
                LogLevel.Debug,
                new EventId(EventIdBase + 4, "PantryUpdated"),
                "{Method}() = {EntityType}.{EntityId} {Entity}");

        private static readonly Action<ILogger, string?, string?, string?, object?, string?, Exception?> _logUpdatedWarning =
            LoggerMessage.Define<string?, string?, string?, object?, string?>(
                LogLevel.Warning,
                new EventId(EventIdWarningBase + 4, "PantryUpdatedWarning"),
                "{Method}() = {EntityType}.{EntityId} {Entity} {Warning}");

        private static readonly Action<ILogger, string?, string?, string?, object?, Exception?> _logDeleted =
            LoggerMessage.Define<string?, string?, string?, object?>(
                LogLevel.Debug,
                new EventId(EventIdBase + 5, "PantryDeleted"),
                "{Method}() = {EntityType}.{EntityId} {Entity}");

        private static readonly Action<ILogger, string?, string?, string?, object?, string?, Exception?> _logDeletedWarning =
            LoggerMessage.Define<string?, string?, string?, object?, string?>(
                LogLevel.Warning,
                new EventId(EventIdWarningBase + 5, "PantryDeletedWarning"),
                "{Method}() = {EntityType}.{EntityId} {Entity} {Warning}");

        private static readonly Action<ILogger, string?, object?, object?, Exception?> _logFind =
            LoggerMessage.Define<string?, object?, object?>(
                LogLevel.Trace,
                new EventId(EventIdBase + 6, "PantryFind"),
                "{Method}() = {Query} {Result}");

        private static readonly Action<ILogger, string?, string?, Exception?> _logClear =
            LoggerMessage.Define<string?, string?>(
                LogLevel.Information,
                new EventId(EventIdBase + 7, "PantryClear"),
                "{Method}() = {EntityType}");

        /// <summary>
        /// Logs a get by id operation.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="entityType">The destination entity type.</param>
        /// <param name="entityId">The destination entity id.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="methodName">The method name, taken from <see cref="CallerMemberNameAttribute"/>.</param>
        public static void LogGetById(
            this ILogger logger,
            Type? entityType,
            string entityId,
            object? entity,
            [CallerMemberName] string? methodName = null)
        {
            _logGetById(
                logger,
                methodName,
                entityType?.Name,
                entityId,
                entity,
                null);
        }

        /// <summary>
        /// Logs an added operation.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="entityType">The destination entity type.</param>
        /// <param name="entityId">The destination entity id.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="methodName">The method name, taken from <see cref="CallerMemberNameAttribute"/>.</param>
        public static void LogAdded(
            this ILogger logger,
            Type? entityType,
            string entityId,
            object entity,
            [CallerMemberName] string? methodName = null)
        {
            _logAdded(
                logger,
                methodName,
                entityType?.Name,
                entityId,
                entity,
                null);
        }

        /// <summary>
        /// Logs an added operation.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="entityType">The destination entity type.</param>
        /// <param name="entityId">The destination entity id.</param>
        /// <param name="warning">The warning message.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="methodName">The method name, taken from <see cref="CallerMemberNameAttribute"/>.</param>
        public static void LogAddedWarning(
            this ILogger logger,
            Type? entityType,
            string entityId,
            string warning,
            object? entity = null,
            Exception? exception = null,
            [CallerMemberName] string? methodName = null)
        {
            _logAddedWarning(
                logger,
                methodName,
                entityType?.Name,
                entityId,
                entity,
                warning,
                exception);
        }

        /// <summary>
        /// Logs an updated operation.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="entityType">The destination entity type.</param>
        /// <param name="entityId">The destination entity id.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="methodName">The method name, taken from <see cref="CallerMemberNameAttribute"/>.</param>
        public static void LogUpdated(
            this ILogger logger,
            Type? entityType,
            string entityId,
            object entity,
            [CallerMemberName] string? methodName = null)
        {
            _logUpdated(
                logger,
                methodName,
                entityType?.Name,
                entityId,
                entity,
                null);
        }

        /// <summary>
        /// Logs an updated operation.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="entityType">The destination entity type.</param>
        /// <param name="entityId">The destination entity id.</param>
        /// <param name="warning">The warning message.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="methodName">The method name, taken from <see cref="CallerMemberNameAttribute"/>.</param>
        public static void LogUpdatedWarning(
            this ILogger logger,
            Type? entityType,
            string entityId,
            string warning,
            object? entity = null,
            Exception? exception = null,
            [CallerMemberName] string? methodName = null)
        {
            _logUpdatedWarning(
                logger,
                methodName,
                entityType?.Name,
                entityId,
                entity,
                warning,
                exception);
        }

        /// <summary>
        /// Logs a deleted operation.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="entityType">The destination entity type.</param>
        /// <param name="entityId">The destination entity id.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="methodName">The method name, taken from <see cref="CallerMemberNameAttribute"/>.</param>
        public static void LogDeleted(
            this ILogger logger,
            Type? entityType,
            string entityId,
            object? entity = null,
            [CallerMemberName] string? methodName = null)
        {
            _logDeleted(
                logger,
                methodName,
                entityType?.Name,
                entityId,
                entity,
                null);
        }

        /// <summary>
        /// Logs a deleted operation.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="entityType">The destination entity type.</param>
        /// <param name="entityId">The destination entity id.</param>
        /// <param name="warning">The warning message.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <param name="methodName">The method name, taken from <see cref="CallerMemberNameAttribute"/>.</param>
        public static void LogDeletedWarning(
            this ILogger logger,
            Type? entityType,
            string entityId,
            string warning,
            object? entity = null,
            Exception? exception = null,
            [CallerMemberName] string? methodName = null)
        {
            _logDeletedWarning(
                logger,
                methodName,
                entityType?.Name,
                entityId,
                entity,
                warning,
                exception);
        }

        /// <summary>
        /// Logs a Find operation.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="methodName">The method name, taken from <see cref="CallerMemberNameAttribute"/>.</param>
        public static void LogFind(
            this ILogger logger,
            object? query,
            object? result,
            [CallerMemberName] string? methodName = null)
        {
            _logFind(
                logger,
                methodName,
                query,
                result,
                null);
        }

        /// <summary>
        /// Logs a Clear operation.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="entityType">The entity type.</param>
        /// <param name="methodName">The method name, taken from <see cref="CallerMemberNameAttribute"/>.</param>
        public static void LogClear(
            this ILogger logger,
            Type entityType,
            [CallerMemberName] string? methodName = null)
        {
            _logClear(
                logger,
                methodName,
                entityType?.Name,
                null);
        }
    }
}
