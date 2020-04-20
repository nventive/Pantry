using System;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
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

        /// <summary>
        /// Configure the PetaPoco Repository to use the <paramref name="connectionString"/>
        /// with the <paramref name="dbProviderFactory"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IPetaPocoRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="dbProviderFactory">The <see cref="DbProviderFactory"/> to use - determines the Database type.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="petaPocoMapper">The custom PetaPoco <see cref="IMapper"/> to use.</param>
        /// <returns>The udpated <see cref="IPetaPocoRepositoryBuilder{TEntity}"/>.</returns>
        public static IPetaPocoRepositoryBuilder<TEntity> WithConnectionString<TEntity>(
            this IPetaPocoRepositoryBuilder<TEntity> builder,
            DbProviderFactory dbProviderFactory,
            string connectionString,
            IMapper? petaPocoMapper = null)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.WithPetaPocoDatabaseFactory(sp => new Database(connectionString, dbProviderFactory, petaPocoMapper));
        }

        /// <summary>
        /// Configure the PetaPoco Repository to use the <paramref name="connectionStringName"/>.
        /// with the <paramref name="dbProviderFactory"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IPetaPocoRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="dbProviderFactory">The <see cref="DbProviderFactory"/> to use - determines the Database type.</param>
        /// <param name="connectionStringName">The name of the connection string.</param>
        /// <param name="petaPocoMapper">The custom PetaPoco <see cref="IMapper"/> to use.</param>
        /// <returns>The udpated <see cref="IPetaPocoRepositoryBuilder{TEntity}"/>.</returns>
        public static IPetaPocoRepositoryBuilder<TEntity> WithConnectionStringNamed<TEntity>(
            this IPetaPocoRepositoryBuilder<TEntity> builder,
            DbProviderFactory dbProviderFactory,
            string connectionStringName,
            IMapper? petaPocoMapper = null)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.WithPetaPocoDatabaseFactory(
                sp => new Database(sp.GetRequiredService<IConfiguration>().GetConnectionString(connectionStringName), dbProviderFactory, petaPocoMapper));
        }
    }
}
