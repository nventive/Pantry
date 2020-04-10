﻿using System.Collections.Concurrent;
using Pantry;
using Pantry.Continuation;
using Pantry.DependencyInjection;
using Pantry.InMemory;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class InMemoryServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default repository implementation backed by a <see cref="ConcurrentDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IRepositoryBuilder"/>.</returns>
        public static IRepositoryBuilder AddConcurrentDictionaryRepository<TEntity>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
            => services.AddConcurrentDictionaryRepository<TEntity, ConcurrentDictionaryRepository<TEntity>>();

        /// <summary>
        /// Adds a custom repository backed by a <see cref="ConcurrentDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TRepository">The repository type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IRepositoryBuilder"/>.</returns>
        public static IRepositoryBuilder AddConcurrentDictionaryRepository<TEntity, TRepository>(this IServiceCollection services)
            where TEntity : class, IIdentifiable, new()
        {
            services.TryAddIdGeneratorFor<TEntity>();
            services.TryAddETagGeneratorFor<TEntity>();
            services.TryAddContinuationTokenEncoderFor<LimitOffsetContinuationToken>();

            services.AddSingleton<ConcurrentDictionary<string, TEntity>>();
            services.TryAddAsSelfAndAllInterfaces<TRepository>();

            return new RepositoryBuilder(services, typeof(TEntity));
        }
    }
}
