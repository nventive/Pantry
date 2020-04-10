﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pantry.Continuation;
using Pantry.Exceptions;

namespace Pantry.Dapper
{
    /// <summary>
    /// Dapper Repository Implementation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class DapperRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IIdentifiable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DapperRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public DapperRepository(
            ILogger<DapperRepository<TEntity>>? logger = null)
        {
            Logger = logger ?? NullLogger<DapperRepository<TEntity>>.Instance;
        }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc/>
        public virtual Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<TEntity> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<TEntity?> TryGetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<bool> TryRemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }

        /// <inheritdoc/>
        public virtual Task<IContinuationEnumerable<TEntity>> FindAllAsync(string? continuationToken, int limit = 50, CancellationToken cancellationToken = default)
        {
            throw new UnsupportedFeatureException("Not supported yet.");
        }
    }
}
