using System;
using Bogus;
using FluentAssertions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Xunit;

namespace Pantry.Tests.Continuation
{
    public class ContinuationTokenTests
    {
        [Fact]
        public void ItShouldConvertTokens()
        {
            var source = new DeserializedToken { Limit = 50, Page = 2 };

            var result = ContinuationToken.FromContinuationToken<DeserializedToken>(
                ContinuationToken.ToContinuationToken(source));

            result.Should().BeEquivalentTo(source);
        }

        [Fact]
        public void ItShouldThrowBadInputExceptionOnMalformedTokens()
        {
            var malformedToken = new Faker().Random.Hash();
            Action act = () => ContinuationToken.FromContinuationToken<DeserializedToken>(malformedToken);

            act.Should().Throw<BadInputException>().WithMessage($"*{malformedToken}*");
        }

        private class DeserializedToken
        {
            public int Limit { get; set; }

            public int Page { get; set; }
        }
    }
}
