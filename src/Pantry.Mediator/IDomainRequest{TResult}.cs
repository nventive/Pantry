namespace Pantry.Mediator
{
    /// <summary>
    /// A request execution that returns a result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface IDomainRequest<out TResult> : IDomainRequest
    {
    }
}
