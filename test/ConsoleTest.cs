using System;
using Xunit;

namespace test
{
    public class ConsoleTest
    {
        [Theory]
        [InlineData("hello")]
        public void SayHello_ReturnsGreeting(string greeting)
        {
            string actual = String.Format("{0}, user!", greeting);
            Assert.Equal("hello, user!", actual);
        }
    }
}
