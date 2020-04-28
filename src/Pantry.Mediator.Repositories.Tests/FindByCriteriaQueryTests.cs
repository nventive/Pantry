using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pantry.Mediator.Repositories.Queries;
using Pantry.Queries;
using Pantry.Tests.StandardTestSupport;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Mediator.Repositories.Tests
{
    public class FindByCriteriaQueryTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public FindByCriteriaQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task ItShouldFindByCriteria()
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
                .TryAddRepositoryHandlersForRequestsInAssemblyContaining<FindByCriteriaQueryTests>()
                .BuildServiceProvider();

            var entities = new[]
            {
                new StandardEntity { Name = "Foo", Age = 18 },
                new StandardEntity { Name = "Bar", Age = 19 },
                new StandardEntity { Name = "FooBar", Age = 20 },
            };

            foreach (var entity in entities)
            {
                await services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);
            }

            var query = new FindByCriteriaQuery { NameEq = "Bar" };
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(query);
            result.Should().HaveCount(1);
            result.First().Should().BeEquivalentTo(entities[1]);
        }

        [Fact]
        public async Task ItShouldFindByCriteriaAndProject()
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
                .TryAddRepositoryHandlersForRequestsInAssemblyContaining<FindByCriteriaQueryTests>()
                .BuildServiceProvider();

            var entities = new[]
            {
                new StandardEntity { Name = "Foo", Age = 18 },
                new StandardEntity { Name = "Bar", Age = 19 },
                new StandardEntity { Name = "FooBar", Age = 20 },
            };

            foreach (var entity in entities)
            {
                await services.GetRequiredService<IRepositoryAdd<StandardEntity>>().AddAsync(entity);
            }

            var query = new FindStandardEntityModelByCriteriaQuery { NameContains = "Foo" };
            var mediator = services.GetRequiredService<IMediator>();

            var result = await mediator.ExecuteAsync(query);
            result.Should().HaveCount(2);
            result.Select(x => x.Name).Should().ContainMatch("*Foo*");
        }

        public class FindByCriteriaQuery : FindByCriteriaDomainQuery<StandardEntity>
        {
            public string? NameEq
            {
                get => this.EqualToValue(x => x.Name);
                set => this.EqualTo(x => x.Name, value);
            }

            public IEnumerable<int>? AgeIn
            {
                get => this.InValue(x => x.Age);
                set => this.In(x => x.Age, value);
            }
        }

        public class FindStandardEntityModelByCriteriaQuery : FindByCriteriaDomainQuery<StandardEntity, StandardEntityModel>
        {
            public string? NameContains
            {
                get => this.StringContainsValue(x => x.Name);
                set => this.StringContains(x => x.Name, value);
            }
        }

        public class StandardEntityModel
        {
            public string? Name { get; set; }
        }
    }
}
