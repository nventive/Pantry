namespace Pantry.Mediator
{
    /// <summary>
    /// A command that returns <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface IDomainCommand<out TResult> : IDomainRequest<TResult>
    {
    }
}
