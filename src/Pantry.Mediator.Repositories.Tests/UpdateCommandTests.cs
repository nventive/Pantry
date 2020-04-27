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
    public class UpdateCommandTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public UpdateCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task ItShouldUpdateAndReturnEntity()
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
                .TryAddRepositoryHandlerForRequestsInAssemblyContaining<CreateCommandTests>()
                .BuildServiceProvider();

            var entity = new StandardEntity { Name = "Bar", Age = 23 };
            entity = await services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var command = new UpdateStandardEntity { Id = entity.Id, Name = "Foo" };
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(command);

            result.Should().NotBeNull();
            result.Id.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Age.Should().Be(23);
        }

        [Fact]
        public async Task ItShouldUpdateAndReturnModel()
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
                .TryAddRepositoryHandlerForRequestsInAssemblyContaining<CreateCommandTests>()
                .BuildServiceProvider();

            var entity = new StandardEntity { Name = "Bar", Age = 23 };
            entity = await services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var command = new UpdateAndProjectStandardEntity { Id = entity.Id, Name = "Foo" };
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(command);

            result.Should().NotBeNull();
            result.Age.Should().Be(23);
        }

        public class UpdateStandardEntity : UpdateCommand<StandardEntity>
        {
            public string? Name { get; set; }
        }

        public class UpdateAndProjectStandardEntity : UpdateCommand<StandardEntity, UpdateStandardEntityModel>
        {
            public string? Name { get; set; }
        }

        public class UpdateStandardEntityModel
        {
            public int Age { get; set; }
        }
    }
}
