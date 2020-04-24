using System;
using Pantry.AspNetCore.Controllers;

namespace Pantry.AspNetCore.Controllers
{
    /// <summary>
    /// Capabilities for a <see cref="RepositoryController{TEntity, TEntityAttributesModel}"/>.
    /// </summary>
    [Flags]
    public enum Capabilities
    {
        /// <summary>
        /// Default value.
        /// </summary>
        None = 0b_0000_0000,

        /// <summary>
        /// Create action (POST /collection).
        /// </summary>
        Create = 0b_0000_0001,

        /// <summary>
        /// GetById action (GET /collection/{id}).
        /// </summary>
        GetById = 0b_0000_0010,

        /// <summary>
        /// Update action (PUT /collection/{id})
        /// </summary>
        Update = 0b_0000_0100,

        /*
        /// <summary>
        /// Update partial action (PATCH /collection/{id})
        /// </summary>
        UpdatePartial = 0b_0000_1000,
        */

        /// <summary>
        /// Delete action (DELETE /collection/{id})
        /// </summary>
        Delete = 0b_0001_0000,

        /// <summary>
        /// All CRUD actions.
        /// </summary>
        CRUD = Create | GetById | Update | Delete,

        /// <summary>
        /// Find action
        /// </summary>
        Find = 0b_0010_0000,

        /// <summary>
        /// All actions.
        /// </summary>
        All = CRUD | Find,
    }
}
