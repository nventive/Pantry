using System;
using Pantry;
using Pantry.DependencyInjection;
using Pantry.Mediator;
using Pantry.Mediator.Repositories.Decorators;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IRepositoryBuilder{TEntity}"/> extension methods.
    /// </summary>
    public static class DomainEventsRepositoryBuilderExtensions
    {
        /// <summary>
        /// Emits standard domain events for repository actions (Add, Update, Delete).
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IRepositoryBuilder{TEntity}"/>.</param>
        /// <returns>The updated <see cref="IRepositoryBuilder{TEntity}"/>.</returns>
        public static IRepositoryBuilder<TEntity> EmitDomainEvents<TEntity>(this IRepositoryBuilder<TEntity> builder)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddRepositoryDecorator(
                (repo, sp) => new DomainEventRepositoryDecorator<TEntity>(
                    sp.GetRequiredService<IMediator>(),
                    repo));

            return builder;
        }
    }
}
