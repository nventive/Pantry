using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry;
using Pantry.PetaPoco;
using Pantry.PetaPoco.DependencyInjection;
using PetaPoco;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IPetaPocoRepositoryBuilder{TEntity}"/> extension methods.
    /// </summary>
    public static class PetaPocoRepositoryBuilderExtensions
    {
        /// <summary>
        /// Configure the PetaPoco Repository to use the <see cref="IDatabase"/>
        /// resolved by the factory.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IPetaPocoRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="databaseFactory">The <see cref="IDatabase"/> factory.</param>
        /// <returns>The udpated <see cref="IPetaPocoRepositoryBuilder{TEntity}"/>.</returns>
        public static IPetaPocoRepositoryBuilder<TEntity> WithPetaPocoDatabaseFactory<TEntity>(
            this IPetaPocoRepositoryBuilder<TEntity> builder,
            Func<IServiceProvider, IDatabase> databaseFactory)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddTransient(sp => new PetaPocoDatabaseFor<TEntity>(databaseFactory(sp)));
            return builder;
        }
    }
}
