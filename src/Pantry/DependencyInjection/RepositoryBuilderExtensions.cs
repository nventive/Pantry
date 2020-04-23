using System;
using System.Linq;
using Pantry;
using Pantry.Decorators;
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
        /// <typeparam name="TDecorator">The type of decorator.</typeparam>
        /// <param name="builder">The <see cref="IRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="decoratorFactory">The decorator factory.</param>
        /// <returns>The updated <see cref="IRepositoryBuilder{TEntity}"/>.</returns>
        public static IRepositoryBuilder<TEntity> AddRepositoryDecorator<TEntity, TDecorator>(
            this IRepositoryBuilder<TEntity> builder,
            Func<object, IServiceProvider, TDecorator> decoratorFactory)
            where TEntity : class, IIdentifiable
            where TDecorator : RepositoryDecorator<TEntity>
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            foreach (var repositoryInterface in builder.RegisteredRepositoryInterfaces.Where(x => x.IsAssignableFrom(typeof(TDecorator))))
            {
                builder.Services.TryDecorate(repositoryInterface, decoratorFactory);
            }

            return builder;
        }
    }
}
