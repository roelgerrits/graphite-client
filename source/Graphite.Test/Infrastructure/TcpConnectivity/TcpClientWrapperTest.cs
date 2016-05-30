using Graphite.Infrastructure;
using Graphite.Infrastructure.TcpConnectivity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Xunit;

namespace Graphite.Test.Infrastructure.TcpConnectivity
{
    public class TcpClientWrapperTest
    {
        [Fact]
        public void ReInitializeShouldReturnFalseWhenNoRetriesAllowedAnymore()
        {
            var tcpClient = new PatchedTcpClient();
            var mockedRetryStrategyMock = new Mock<IRetry>();
            mockedRetryStrategyMock.Setup(strategy => strategy.RetryAllowed()).Returns(false);
            var tcpClientProperties = new TcpClientProperties(IPAddress.Parse("127.0.0.1"), 2001, 10);
            var tcpWrapper = new TcpClientWrapper(tcpClientProperties, tcpClient, mockedRetryStrategyMock.Object);

            var result = tcpWrapper.ReInitialize();

            Assert.False(result);
            mockedRetryStrategyMock.Verify(x => x.RetryAllowed(), Times.Once());
        }

        [Fact]
        public void ReInitializeShouldReturnTrueWhenRetriesAllowed()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2001);
            var tcpClient = new PatchedTcpClient();
            var mockedRetryStrategyMock = new Mock<IRetry>();
            mockedRetryStrategyMock.Setup(strategy => strategy.RetryAllowed()).Returns(true);
            var tcpClientProperties = new TcpClientProperties(IPAddress.Parse("127.0.0.1"), 2001, 10);
            var tcpWrapper = new TcpClientWrapper(tcpClientProperties, tcpClient, mockedRetryStrategyMock.Object);

            var result = tcpWrapper.ReInitialize();

            Assert.True(result);
            mockedRetryStrategyMock.Verify(x => x.RetryAllowed(), Times.Once());
        }

        [Fact]
        public void WriteShouldCallResetOnRetryStrategyWhenOk()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2001);
            var mockedTcpClient = new Mock<PatchedTcpClient>();
            var mockedRetryStrategyMock = new Mock<IRetry>();
            var tcpClientProperties = new TcpClientProperties(IPAddress.Parse("127.0.0.1"), 2001, 10);
            var tcpWrapper = new TcpClientWrapper(tcpClientProperties, mockedTcpClient.Object, mockedRetryStrategyMock.Object);
            var data = Encoding.Default.GetBytes("data");

            tcpWrapper.Write(data, 1, 1);

            mockedRetryStrategyMock.Verify(x => x.Reset(), Times.Once());
        }
    }
}
