using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Pantry.Exceptions;
using Pantry.Traits;
using Xunit;
using Xunit.Abstractions;

namespace Pantry.Tests.StandardTestSupport
{
    /// <summary>
    /// Base class for standard test suit that repository implementation can use & follow.
    /// </summary>
    /// <typeparam name="TRepository">The type of repository to test.</typeparam>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "OK Here - support for generic test suite / xUnit member data pattern.")]
    public abstract class StandardRepositoryImplementationTests<TRepository> : StandardRepositoryImplementationTests<TRepository, StandardEntity>
    {
        public StandardRepositoryImplementationTests(
            StandardRepositoryImplementationTestsFixture fixture,
            ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        public static IEnumerable<object[]> ItShouldFindWithASimpleCriteriaQueryData
            => new List<Func<StandardEntity, StandardEntityCriteriaQuery>>
                {
                    x => new StandardEntityCriteriaQuery { IdEq = x.Id },
                    x => new StandardEntityCriteriaQuery { NameEq = x.Name },
                    x => new StandardEntityCriteriaQuery { NameLike = x.Name!.Substring(1, 3) },
                    x => new StandardEntityCriteriaQuery { NameNeq = $"{x.Name}notsamename" },
                    x => new StandardEntityCriteriaQuery { AgeEq = x.Age },
                    x => new StandardEntityCriteriaQuery { AgeGt = x.Age - 1 },
                    x => new StandardEntityCriteriaQuery { AgeGte = x.Age },
                    x => new StandardEntityCriteriaQuery { AgeLt = x.Age + 1 },
                    x => new StandardEntityCriteriaQuery { AgeLte = x.Age },
                    x => new StandardEntityCriteriaQuery { AgeIn = new[] { x.Age, x.Age + 2 } },
                    x => new StandardEntityCriteriaQuery { NotarizedAtEq = x.NotarizedAt },
                    x => new StandardEntityCriteriaQuery { NotarizedAtGt = x.NotarizedAt!.Value.AddDays(-1) },
                    x => new StandardEntityCriteriaQuery { NotarizedAtGte = x.NotarizedAt!.Value.AddDays(-1) },
                    x => new StandardEntityCriteriaQuery { NotarizedAtLt = x.NotarizedAt!.Value.AddDays(1) },
                    x => new StandardEntityCriteriaQuery { NotarizedAtLte = x.NotarizedAt!.Value.AddDays(1) },
                    x => new StandardEntityCriteriaQuery { IsNotarized = true },
                    x => new StandardEntityCriteriaQuery { RelatedNameEq = x.Related!.Name },
                    x => new StandardEntityCriteriaQuery { RelatedNameLike = x.Related!.Name!.Substring(0, 1) },
                    x => new StandardEntityCriteriaQuery { RelatedNameIn = new[] { x.Related!.Name, $"{x.Related!.Name}withother" } },
                    x => new StandardEntityCriteriaQuery { LinesNameEq = x.Lines[0].Name },
                    x => new StandardEntityCriteriaQuery { LinesNameLike = x.Lines[0].Name!.Substring(0, 1) },
                }.Select(x => new object[] { x });

        public static IEnumerable<object[]> ItShouldFindWithACombinedCriteriaQueryData
            => new List<Func<StandardEntity, StandardEntityCriteriaQuery>>
                {
                    x => new StandardEntityCriteriaQuery { NameEq = x.Name, AgeGt = x.Age - 1, NotarizedAtLt = x.NotarizedAt!.Value.AddDays(1) },
                    x => new StandardEntityCriteriaQuery { IdEq = x.Id, NotarizedAtLte = x.NotarizedAt!.Value.AddDays(1) },
                    x => new StandardEntityCriteriaQuery { NotarizedAtLte = x.NotarizedAt!.Value.AddDays(10), OrderBy = $"-{nameof(StandardEntity.Age)}" },
                    x => new StandardEntityCriteriaQuery { AgeGt = x.Age - 1, OrderBy = $"+{nameof(StandardEntity.Name)}" },
                }.Select(x => new object[] { x });

        protected virtual Faker Faker { get; } = new Faker();

        protected override Faker<StandardEntity> TestEntityGenerator => new Faker<StandardEntity>()
            .RuleFor(x => x.Name, f => f.Person.UserName)
            .RuleFor(x => x.Age, f => f.Random.Int(1, 100))
            .RuleFor(x => x.NotarizedAt, f => f.Date.PastOffset())
            .RuleFor(x => x.Related, f => SubTestEntityGenerator.Generate())
            .RuleFor(x => x.Lines, f => SubTestEntityGenerator.Generate(3).ToList());

        protected virtual Faker<SubStandardEntity> SubTestEntityGenerator => new Faker<SubStandardEntity>()
            .RuleFor(x => x.Name, f => f.Lorem.Word());

        [SkippableTheory(typeof(UnsupportedFeatureException))]
        [MemberData(nameof(ItShouldFindWithASimpleCriteriaQueryData))]
        public virtual async Task ItShouldFindWithASimpleCriteriaQuery(Func<StandardEntity, StandardEntityCriteriaQuery> queryFactory)
        {
            if (queryFactory is null)
            {
                throw new ArgumentNullException(nameof(queryFactory));
            }

            var repo = GetRepositoryAs<IRepositoryFindByCriteria<StandardEntity>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<StandardEntity>(repo, entities);

            var targetEntity = Faker.PickRandom(entities);
            var query = queryFactory(targetEntity);
            var result = await repo.FindAsync(query);

            result.Should().HaveCountGreaterOrEqualTo(1);
            result.Select(x => x.Id).Should().Contain(targetEntity.Id);
        }

        [SkippableTheory(typeof(UnsupportedFeatureException))]
        [MemberData(nameof(ItShouldFindWithACombinedCriteriaQueryData))]
        public virtual async Task ItShouldFindWithACombinedCriteriaQuery(Func<StandardEntity, StandardEntityCriteriaQuery> queryFactory)
        {
            if (queryFactory is null)
            {
                throw new ArgumentNullException(nameof(queryFactory));
            }

            var repo = GetRepositoryAs<IRepositoryFindByCriteria<StandardEntity>>();
            var entities = TestEntityGenerator.Generate(5);
            using var scope = new TemporaryEntitiesScope<StandardEntity>(repo, entities);

            var targetEntity = Faker.PickRandom(entities);
            var query = queryFactory(targetEntity);
            var result = await repo.FindAsync(query);

            result.Should().HaveCountGreaterOrEqualTo(1);
            result.Select(x => x.Id).Should().Contain(targetEntity.Id);
        }
    }
}
