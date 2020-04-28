using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pantry.Mediator.Repositories.Queries;
using Pantry.Tests.StandardTestSupport;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Mediator.Repositories.Tests
{
    public class GetByIdQueryTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public GetByIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task ItShouldGetById()
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
                .TryAddRepositoryHandlersForRequestsInAssemblyContaining<GetByIdQueryTests>()
                .BuildServiceProvider();

            var entity = new StandardEntity { Name = "Foo" };
            entity = await services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var query = new GetStandardEntityByIdQuery { Id = entity.Id };
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(query);
            result.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public async Task ItShouldGetByIdAndProject()
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
                .TryAddRepositoryHandlersForRequestsInAssemblyContaining<GetByIdQueryTests>()
                .BuildServiceProvider();

            var entity = new StandardEntity { Name = "Foo" };
            entity = await services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);

            var query = new GetStandardEntityModelByIdQuery { Id = entity.Id };
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(query);
            result.Name.Should().Be(entity.Name);
        }

        public class GetStandardEntityByIdQuery : GetByIdDomainQuery<StandardEntity>
        {
        }

        public class GetStandardEntityModelByIdQuery : GetByIdDomainQuery<StandardEntity, StandardEntityModel>
        {
        }

        public class StandardEntityModel
        {
            public string? Name { get; set; }
        }
    }
}
