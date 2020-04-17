using FluentAssertions;
using Pantry.Reflection;
using Pantry.Tests.StandardTestSupport;
using Xunit;

namespace Pantry.Tests.Reflection
{
    public class PropertyVisitorTests
    {
        [Fact]
        public void ItShouldGetSimplePropertyPath()
        {
            var result = PropertyVisitor.GetPropertyPath((RootAggregateEntity x) => x.ETag);
            result.Should().Be(nameof(RootAggregateEntity.ETag));
        }

        [Fact]
        public void ItShouldGetMoreComplexPropertyPath()
        {
            var result = PropertyVisitor.GetPropertyPath((StandardEntity x) => x.Related!.Name);
            result.Should().Be($"{nameof(StandardEntity.Related)}.{nameof(SubStandardEntity.Name)}");
        }

        [Fact]
        public void ItShouldGetMoreComplexPropertyPathWithIndexers()
        {
            var result = PropertyVisitor.GetPropertyPath((StandardEntity x) => x.Lines[0].Name);
            result.Should().Be($"{nameof(StandardEntity.Lines)}[].{nameof(SubStandardEntity.Name)}");
        }
    }
}
