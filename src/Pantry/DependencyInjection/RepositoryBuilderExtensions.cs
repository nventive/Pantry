using System;
using Pantry;
using Pantry.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder{TEntity}"/> extensions.
    /// </summary>
    public static class RepositoryBuilderExtensions
    {
        /// <summary>
        /// Adds a repository decorator.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="decoratorFactory">The decorator factory.</param>
        /// <returns>The updated <see cref="IRepositoryBuilder{TEntity}"/>.</returns>
        public static IRepositoryBuilder<TEntity> AddRepositoryDecorator<TEntity>(
            this IRepositoryBuilder<TEntity> builder,
            Func<object, IServiceProvider, object> decoratorFactory)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            foreach (var repositoryInterface in builder.RegisteredRepositoryInterfaces)
            {
                builder.Services.TryDecorate(repositoryInterface, decoratorFactory);
            }

            return builder;
        }
    }
}
