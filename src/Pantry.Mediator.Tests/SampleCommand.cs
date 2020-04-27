using System.Threading;
using System.Threading.Tasks;

namespace Pantry.Mediator.Tests
{
    public class SampleCommand : IDomainQuery<SampleEntity>
    {
        public class Handler : IDomainRequestHandler<SampleCommand, SampleEntity>
        {
            public async Task<SampleEntity> HandleAsync(SampleCommand request, CancellationToken cancellationToken)
            {
                return new SampleEntity { Id = GetType().Name };
            }
        }
    }
}
