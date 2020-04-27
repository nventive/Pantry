using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;

namespace Pantry.Mediator.FluentValidation
{
    /// <summary>
    /// <see cref="IDomainRequestMiddleware"/> that performs validation on all requests.
    /// </summary>
    public class FluentValidationDomainRequestMiddleware : IDomainRequestMiddleware
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidationDomainRequestMiddleware"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public FluentValidationDomainRequestMiddleware(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public async Task<object> HandleAsync(IDomainRequest request, Func<IDomainRequest, Task<object>> nextHandler, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (nextHandler is null)
            {
                throw new ArgumentNullException(nameof(nextHandler));
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
            var validator = (IValidator)_serviceProvider.GetService(validatorType);
            if (validator != null)
            {
                var validationContext = new ValidationContext(
                    request,
                    new PropertyChain(),
                    ValidatorOptions.ValidatorSelectors.RulesetValidatorSelectorFactory(new[] { "default", request.GetType().Name }));
                var result = await validator.ValidateAsync(validationContext, cancellationToken).ConfigureAwait(false);
                if (!result.IsValid)
                {
                    throw new ValidationException(result.Errors);
                }
            }

            return await nextHandler(request).ConfigureAwait(false);
        }
    }
}
