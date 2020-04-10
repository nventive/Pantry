using System;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Pantry.Continuation;
using Pantry.Exceptions;
using Xunit;

namespace Pantry.Tests.Continuation
{
    public class Base64JsonContinuationTokenEncoderTests
    {
        [Fact]
        public async Task ItShouldConvertTokens()
        {
            var encoder = new Base64JsonContinuationTokenEncoder<TestToken>();
            var source = new TestToken { Limit = 50, Page = 2 };

            var result = await encoder.Decode(await encoder.Encode(source));

            result.Should().BeEquivalentTo(source);
        }

        [Fact]
        public void ItShouldThrowBadInputExceptionOnMalformedTokens()
        {
            var encoder = new Base64JsonContinuationTokenEncoder<TestToken>();
            var malformedToken = new Faker().Random.Hash();
            Func<Task> act = async () => await encoder.Decode(malformedToken);

            act.Should().Throw<BadInputException>().WithMessage($"*{malformedToken}*");
        }

        private class TestToken
        {
            public int Limit { get; set; }

            public int Page { get; set; }
        }
    }
}
