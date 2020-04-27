using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry.Exceptions;
using Pantry.Mediator;
using Pantry.Mediator.Repositories;
using Pantry.Mediator.Repositories.Commands;
using Pantry.Mediator.Repositories.Handlers;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class MediatorRepositoriesServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the corresponding repository handler for <paramref name="requestType"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="requestType">The request type. Must be a <see cref="IRepositoryRequest"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddRepositoryHandler(this IServiceCollection services, Type requestType)
        {
            if (requestType is null)
            {
                throw new ArgumentNullException(nameof(requestType));
            }

            if (!typeof(IRepositoryRequest).IsAssignableFrom(requestType))
            {
                throw new ArgumentException($"Unable to add repository handlers for type {requestType} as it is not a IRepositoryRequest.", nameof(requestType));
            }

            var currentRequestType = requestType;
            do
            {
#pragma warning disable CA1062 // Validate arguments of public methods - there seems to be a bug in the analyzer.
                if (currentRequestType.IsGenericType)
                {
                    var genericTypeDefinition = currentRequestType.GetGenericTypeDefinition();
                    if (typeof(CreateCommand<,>) == genericTypeDefinition)
                    {
                        var commandArgs = currentRequestType.GenericTypeArguments;
                        var serviceType = typeof(IDomainRequestHandler<,>).MakeGenericType(requestType, commandArgs[1]);
                        var implementationType = typeof(CreateCommandRepositoryHandler<,,>).MakeGenericType(commandArgs[0], requestType, commandArgs[1]);
                        services.TryAdd(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient));
                        return services;
                    }

                    if (typeof(UpdateCommand<,>) == genericTypeDefinition)
                    {
                        var commandArgs = currentRequestType.GenericTypeArguments;
                        var serviceType = typeof(IDomainRequestHandler<,>).MakeGenericType(requestType, commandArgs[1]);
                        var implementationType = typeof(UpdateCommandRepositoryHandler<,,>).MakeGenericType(commandArgs[0], requestType, commandArgs[1]);
                        services.TryAdd(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient));
                        return services;
                    }

                    if (typeof(DeleteCommand<>) == genericTypeDefinition)
                    {
                        var commandArgs = currentRequestType.GenericTypeArguments;
                        var serviceType = typeof(IDomainRequestHandler<,>).MakeGenericType(requestType, typeof(bool));
                        var implementationType = typeof(DeleteCommandRepositoryHandler<,>).MakeGenericType(commandArgs[0], requestType);
                        services.TryAdd(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient));
                        return services;
                    }
                }

                currentRequestType = currentRequestType.BaseType;
            }
            while (currentRequestType != typeof(object));

            throw new InternalErrorException($"Unable to find a suitable repository handler for {requestType}.");
        }

        /// <summary>
        /// Registers the corresponding repository handler for <typeparamref name="TRequest"/>.
        /// </summary>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddRepositoryHandler<TRequest>(this IServiceCollection services)
            where TRequest : IRepositoryRequest
            => services.TryAddRepositoryHandler(typeof(TRequest));

        /// <summary>
        /// Tries to register the corresponding repository handler for all <see cref="IRepositoryRequest"/> in <paramref name="assembly"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddRepositoryHandlerForRequestsInAssembly(this IServiceCollection services, Assembly assembly)
        {
            var requestTypes = assembly.GetTypes()
                .Where(x => !x.IsGenericType && !x.IsAbstract)
                .Where(x => typeof(IRepositoryRequest).IsAssignableFrom(x));

            foreach (var requestType in requestTypes)
            {
                services.TryAddRepositoryHandler(requestType);
            }

            return services;
        }

        /// <summary>
        /// Tries to register the corresponding repository handlers for all <see cref="IRepositoryRequest"/> in
        /// the assembly of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type in the assembly to scan.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddRepositoryHandlerForRequestsInAssemblyContaining<T>(this IServiceCollection services)
            => services.TryAddRepositoryHandlerForRequestsInAssembly(typeof(T).Assembly);
    }
}
