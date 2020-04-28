using System.Linq;
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
    public class CreateCommandTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public CreateCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task ItShouldCreateAndReturnEntity()
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
                .TryAddRepositoryHandlersForRequestsInAssemblyContaining<CreateCommandTests>()
                .BuildServiceProvider();

            var command = new CreateStandardEntity { Name = "Foo", Age = 23 };
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(command);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Age.Should().Be(command.Age);

            var entity = await services.GetRequiredService<IRepositoryGet<StandardEntity>>().GetByIdAsync(result.Id);
            result.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public async Task ItShouldCreateAndReturnModel()
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
                .TryAddRepositoryHandlersForRequestsInAssemblyContaining<CreateCommandTests>()
                .BuildServiceProvider();

            var command = new CreateAndProjectStandardEntity { Name = "Foo", Age = 23 };
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(command);

            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);

            var entity = (await services.GetRequiredService<IRepositoryFindAll<StandardEntity>>().FindAllAsync(null)).First();
            result.Name.Should().Be(entity.Name);
        }

        public class CreateStandardEntity : CreateCommand<StandardEntity>
        {
            public string? Name { get; set; }

            public int Age { get; set; }
        }

        public class CreateAndProjectStandardEntity : CreateCommand<StandardEntity, CreateStandardEntityModel>
        {
            public string? Name { get; set; }

            public int Age { get; set; }
        }

        public class CreateStandardEntityModel
        {
            public string? Name { get; set; }
        }
    }
}
