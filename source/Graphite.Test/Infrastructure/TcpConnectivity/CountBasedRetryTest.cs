using Graphite.Infrastructure.TcpConnectivity;
using Xunit;

namespace Graphite.Test.Infrastructure.TcpConnectivity
{
    public class CountBasedRetryTest
    {
        [Fact]
        public void RetryAllowedShouldOnlyAllowThreeRetriesWhenConfigured()
        {
            var countBasedRetry = new CountBasedRetry(3);

            var result1 = countBasedRetry.RetryAllowed();
            var result2 = countBasedRetry.RetryAllowed();
            var result3 = countBasedRetry.RetryAllowed();
            var result4 = countBasedRetry.RetryAllowed();

            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);
            Assert.False(result4);
        }

        public void RetryAllowedShouldNotAllowRetryWhenZeroRetriesConfigured()
        {
            var countBasedRetry = new CountBasedRetry(0);

            var result1 = countBasedRetry.RetryAllowed();

            Assert.False(result1);
        }

        public void RetryAllowedShouldReturnTrueWhenMinusOneConfigured()
        {
            var countBasedRetry = new CountBasedRetry(-1);

            var result1 = countBasedRetry.RetryAllowed();
            var result2 = countBasedRetry.RetryAllowed();
            var result3 = countBasedRetry.RetryAllowed();
            var result4 = countBasedRetry.RetryAllowed();

            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);
            Assert.True(result4);
        }
    }
}
