namespace Pantry
{
    /// <summary>
    /// Base class for entities that are <see cref="IIdentifiable"/>.
    /// </summary>
    public abstract class Entity : IIdentifiable
    {
        /// <inheritdoc/>
        public string Id { get; set; } = string.Empty;

        /// <inheritdoc />
        public override string ToString() => $"[{GetType().Name}] ({(string.IsNullOrEmpty(Id) ? "<new>" : Id)})";
    }
}
