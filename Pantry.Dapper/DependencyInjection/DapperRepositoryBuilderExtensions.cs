using System;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pantry;
using Pantry.Dapper;
using Pantry.Dapper.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IDapperRepositoryBuilder{TEntity}"/> extension methods.
    /// </summary>
    public static class DapperRepositoryBuilderExtensions
    {
        /// <summary>
        /// Use a <see cref="DbConnection"/> factory for connectivity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IDapperRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="dbConnectionFactory">The <see cref="DbConnection"/> factory.</param>
        /// <returns>The updated <see cref="IDapperRepositoryBuilder{TEntity}"/>.</returns>
        public static IDapperRepositoryBuilder<TEntity> WithDbConnectionFactory<TEntity>(
            this IDapperRepositoryBuilder<TEntity> builder,
            Func<IServiceProvider, DbConnection> dbConnectionFactory)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (dbConnectionFactory is null)
            {
                throw new ArgumentNullException(nameof(dbConnectionFactory));
            }

            builder.Services.TryAddScoped(sp =>
            {
                return new DbConnectionFor<TEntity>(dbConnectionFactory(sp));
            });
            return builder;
        }

        /// <summary>
        /// Use a connection string and provider name.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IDapperRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="dbProviderFactory">The <see cref="DbProviderFactory"/> (e.g. SQLiteFactory.Instance or SqlClientFactory.Instance).</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The updated <see cref="IDapperRepositoryBuilder{TEntity}"/>.</returns>
        public static IDapperRepositoryBuilder<TEntity> WithConnectionString<TEntity>(
            this IDapperRepositoryBuilder<TEntity> builder,
            DbProviderFactory dbProviderFactory,
            string connectionString)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (dbProviderFactory is null)
            {
                throw new ArgumentNullException(nameof(dbProviderFactory));
            }

            builder.WithDbConnectionFactory(sp =>
            {
                var dbConnection = dbProviderFactory.CreateConnection();
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                return dbConnection;
            });

            return builder;
        }

        /// <summary>
        /// Use a connection string name (from <see cref="IConfiguration"/>) and provider name.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="builder">The <see cref="IDapperRepositoryBuilder{TEntity}"/>.</param>
        /// <param name="dbProviderFactory">The <see cref="DbProviderFactory"/> (e.g. SQLiteFactory.Instance or SqlClientFactory.Instance).</param>
        /// <param name="connectionStringName">The connection stringName.</param>
        /// <returns>The updated <see cref="IDapperRepositoryBuilder{TEntity}"/>.</returns>
        public static IDapperRepositoryBuilder<TEntity> WithConnectionStringNamed<TEntity>(
            this IDapperRepositoryBuilder<TEntity> builder,
            DbProviderFactory dbProviderFactory,
            string connectionStringName)
            where TEntity : class, IIdentifiable
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (dbProviderFactory is null)
            {
                throw new ArgumentNullException(nameof(dbProviderFactory));
            }

            builder.WithDbConnectionFactory(sp =>
            {
                var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(connectionStringName);
                var dbConnection = dbProviderFactory.CreateConnection();
                dbConnection.ConnectionString = connectionString;
                dbConnection.Open();
                return dbConnection;
            });

            return builder;
        }
    }
}
