using System.Threading;
using System.Threading.Tasks;
using Pantry.Continuation;

namespace Pantry.Mediator.Tests
{
    public class SampleQuery : IDomainQuery<IContinuationEnumerable<SampleEntity>>
    {
        public class Handler : IDomainRequestHandler<SampleQuery, IContinuationEnumerable<SampleEntity>>
        {
            public async Task<IContinuationEnumerable<SampleEntity>> HandleAsync(SampleQuery request, CancellationToken cancellationToken)
            {
                return new ContinuationEnumerable<SampleEntity>(new[] { new SampleEntity { Id = GetType().Name } });
            }
        }
    }
}
