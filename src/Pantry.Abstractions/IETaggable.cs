namespace Pantry
{
    /// <summary>
    /// Has an ETag / Version property.
    /// </summary>
    public interface IETaggable
    {
        /// <summary>
        /// Gets or sets the ETag.
        /// </summary>
        string? ETag { get; set; }
    }
}
