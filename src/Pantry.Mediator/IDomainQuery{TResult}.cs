namespace Pantry.Mediator
{
    /// <summary>
    /// A query that returns <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface IDomainQuery<out TResult> : IDomainRequest<TResult>
    {
    }
}
