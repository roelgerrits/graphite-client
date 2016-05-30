using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Graphite.Infrastructure;
using Xunit;
using Moq;
using Graphite.Infrastructure.TcpConnectivity;

namespace Graphite.Test.Infrastructure
{
    public class TcpSenderPipeTests
    {
        [Fact]
        public void SendOneMessageShouldCallTcpClientWrappersWrite()
        {
            var mockedTcpClientWrapper = new Mock<ITcpClientWrapper>();
            mockedTcpClientWrapper
                .SetupGet(client => client.Connected).Returns(false);
            mockedTcpClientWrapper
                .Setup(client => client.Connect())
                .Callback(() => mockedTcpClientWrapper.SetupGet(client => client.Connected).Returns(true));
            var sender = new TcpSenderPipe(mockedTcpClientWrapper.Object);

            mockedTcpClientWrapper.Setup(client => client.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => sender.TokenSource.Cancel());
            
            sender.Send("first message");
            var task = sender.Run();
            task.Wait();

            mockedTcpClientWrapper.Verify(x => x.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
        }

        [Fact]
        public void SendShouldContinueWhenInvalidOperationExceptionIsThrownAndReInitializeOk()
        {
            var mockedTcpClientWrapper = new Mock<ITcpClientWrapper>();
            mockedTcpClientWrapper
                .SetupGet(client => client.Connected).Returns(false);
            mockedTcpClientWrapper
                .Setup(client => client.Connect())
                .Callback(() => mockedTcpClientWrapper.SetupGet(client => client.Connected).Returns(true));
            mockedTcpClientWrapper
                .Setup(client => client.ReInitialize()).Returns(true);

            var sender = new TcpSenderPipe(mockedTcpClientWrapper.Object);

            var secondMessage = Encoding.Default.GetBytes("second message\n");
            mockedTcpClientWrapper.Setup(client => client.Write(It.Is<byte[]>(bytes => bytes.SequenceEqual(secondMessage)), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new InvalidOperationException());

            var thirdMessage = Encoding.Default.GetBytes("third message\n");
            mockedTcpClientWrapper.Setup(client => client.Write(It.Is<byte[]>(bytes => bytes.SequenceEqual(thirdMessage)), It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => sender.TokenSource.Cancel());

            sender.Send("first message");
            sender.Send("second message");
            sender.Send("third message");

            var task = sender.Run();
            task.Wait();

            mockedTcpClientWrapper.Verify(client => client.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(3));
        }

        [Fact]
        public void SendShouldContinueWhenSocketExceptionIsThrownAndReInitializeOk()
        {
            var mockedTcpClientWrapper = new Mock<ITcpClientWrapper>();
            mockedTcpClientWrapper
                .SetupGet(client => client.Connected).Returns(false);
            mockedTcpClientWrapper
                .Setup(client => client.Connect())
                .Callback(() => mockedTcpClientWrapper.SetupGet(client => client.Connected).Returns(true));
            mockedTcpClientWrapper
                .Setup(client => client.ReInitialize()).Returns(true);

            var sender = new TcpSenderPipe(mockedTcpClientWrapper.Object);

            var secondMessage = Encoding.Default.GetBytes("second message\n");
            mockedTcpClientWrapper.Setup(client => client.Write(It.Is<byte[]>(bytes => bytes.SequenceEqual(secondMessage)), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new SocketException());

            var thirdMessage = Encoding.Default.GetBytes("third message\n");
            mockedTcpClientWrapper.Setup(client => client.Write(It.Is<byte[]>(bytes => bytes.SequenceEqual(thirdMessage)), It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => sender.TokenSource.Cancel());

            sender.Send("first message");
            sender.Send("second message");
            sender.Send("third message");

            var task = sender.Run();
            task.Wait();

            mockedTcpClientWrapper.Verify(client => client.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(3));
        }

        [Fact]
        public void SendShouldThrowExceptionWhenReInitializeReturnsFalse()
        {
            var mockedTcpClientWrapper = new Mock<ITcpClientWrapper>();
            mockedTcpClientWrapper
                .SetupGet(client => client.Connected).Returns(false);
            mockedTcpClientWrapper
                .Setup(client => client.Connect())
                .Callback(() => mockedTcpClientWrapper.SetupGet(client => client.Connected).Returns(true));
            mockedTcpClientWrapper
                .Setup(client => client.ReInitialize()).Returns(false);

            var sender = new TcpSenderPipe(mockedTcpClientWrapper.Object);
            mockedTcpClientWrapper.Setup(client => client.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new SocketException());
            
            sender.Send("first message");
            
            var exception = Record.Exception(() => {
                var task = sender.Run();
                task.Wait();
            });

            Assert.IsType<SocketException>(exception.InnerException);
        }
    }
}
