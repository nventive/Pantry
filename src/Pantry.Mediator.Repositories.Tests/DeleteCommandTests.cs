using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pantry.Mediator.Repositories.Commands;
using Pantry.Tests.StandardTestSupport;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Mediator.Repositories.Tests
{
    public class DeleteCommandTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public DeleteCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task ItShouldDelete()
        {
            var services = new ServiceCollection()
                .AddLogging(logging =>
                {
                    logging.AddXUnit(_outputHelper).AddFilter(_ => true);
                })
                .AddConcurrentDictionaryRepository<StandardEntity>()
                .EmitDomainEvents()
                .Services
                .AddMediator()
                .TryAddRepositoryHandlersForRequestsInAssemblyContaining<DeleteCommandTests>()
                .BuildServiceProvider();

            var entity = new StandardEntity();
            entity = await services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var command = new DeleteStandardEntity { Id = entity.Id };
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(command);

            result.Should().BeTrue();

            var verifyEntity = await services.GetRequiredService<IRepositoryGet<StandardEntity>>().TryGetByIdAsync(entity.Id);
            verifyEntity.Should().BeNull();
        }

        public class DeleteStandardEntity : DeleteCommand<StandardEntity>
        {
        }
    }
}
