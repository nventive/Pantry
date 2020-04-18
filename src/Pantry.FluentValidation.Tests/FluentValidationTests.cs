using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pantry.Tests.StandardTestSupport;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.FluentValidation.Tests
{
    public class FluentValidationTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public FluentValidationTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task ItShouldValidateSuccessfully()
        {
            var services = GetServiceCollectionWithFluentValidation();
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ICrudRepository<StandardEntity>>();
            var entity = new StandardEntity
            {
                Age = 18,
            };

            var result = await repo.AddAsync(entity);
            result.Should().NotBeNull();

            entity = new StandardEntity
            {
                Id = result.Id,
                Age = 21,
            };

            result = await repo.UpdateAsync(entity);
            result.Should().NotBeNull();

            entity = new StandardEntity
            {
                Id = result.Id,
                Age = 23,
            };

            var (aoUresult, _) = await repo.AddOrUpdateAsync(entity);
            aoUresult.Should().NotBeNull();
        }

        [Fact]
        public async Task ItShouldPerformValidation()
        {
            var services = GetServiceCollectionWithFluentValidation();
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ICrudRepository<StandardEntity>>();
            var entity = new StandardEntity
            {
                Age = 16,
            };

            Func<Task> act = async () => await repo.AddAsync(entity);

            act.Should().Throw<ValidationException>();

            entity = new StandardEntity
            {
                Age = 20,
            };

            act = async () => await repo.UpdateAsync(entity);

            act.Should().Throw<ValidationException>();

            entity = new StandardEntity
            {
                Age = 200,
            };

            act = async () => await repo.AddOrUpdateAsync(entity);

            act.Should().Throw<ValidationException>();
        }

        private IServiceCollection GetServiceCollectionWithFluentValidation()
        {
            var svc = new ServiceCollection();
            svc
                .AddLogging(logging =>
                {
                    logging
                        .AddXUnit(_outputHelper)
                        .AddFilter(_ => true);
                })
                .AddValidatorsFromAssemblyContaining<FluentValidationTests>()
                .AddConcurrentDictionaryRepository<StandardEntity>()
                .WithFluentValidation();

            return svc;
        }
    }
}
