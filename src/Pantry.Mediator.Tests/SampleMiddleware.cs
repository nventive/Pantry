using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pantry.Mediator.Tests
{
    public class SampleMiddleware : IDomainRequestMiddleware
    {
        private readonly string _addToId;

        public SampleMiddleware(string addToId)
        {
            _addToId = addToId;
        }

        public async Task<object> HandleAsync(IDomainRequest request, Func<IDomainRequest, Task<object>> nextHandler, CancellationToken cancellationToken)
        {
            if (nextHandler is null)
            {
                throw new ArgumentNullException(nameof(nextHandler));
            }

            if (string.IsNullOrEmpty(_addToId))
            {
                return new SampleEntity { Id = "shortcut" };
            }

            var result = (SampleEntity)await nextHandler(request);
            result.Id += _addToId;
            return result;
        }
    }
}
