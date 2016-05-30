using System.Net;

namespace Graphite.Infrastructure.TcpConnectivity
{
    /// <summary>
    /// TcpClientWrapper around the tcp client with retry functionality
    /// </summary>
    public interface ITcpClientWrapper
    {
        /// <summary>
        /// Close
        /// </summary>
        void Close();
        /// <summary>
        /// Connected
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// Connect
        /// </summary>
        void Connect();
        /// <summary>
        /// ReInitialize, recreates the tcpclient
        /// </summary>
        /// <returns></returns>
        bool ReInitialize();
        /// <summary>
        /// Write byte array to tcp connection
        /// </summary>
        /// <param name="data"></param>
        /// <param name="i"></param>
        /// <param name="length"></param>
        void Write(byte[] data, int i, int length);
    }

    /// <summary>
    /// TcpClientWrapper around the tcp client with retry functionality
    /// </summary>
    public class TcpClientWrapper : ITcpClientWrapper
    {
        private IRetry _retryStrategy;
        private IPatchedTcpClient _tcpClient;
        private IPEndPoint _ipEndpoint;

        /// <summary>
        /// TcpClientWrapper around the tcp client with retry functionality
        /// </summary>
        /// <param name="tcpClientProperties"></param>
        /// <param name="patchedTcpClient"></param>
        /// <param name="retryStrategy"></param>
        public TcpClientWrapper(TcpClientProperties tcpClientProperties, IPatchedTcpClient patchedTcpClient, IRetry retryStrategy)
        {
            _ipEndpoint = new IPEndPoint(tcpClientProperties.Address, tcpClientProperties.Port);
            _tcpClient = patchedTcpClient;
            _retryStrategy = retryStrategy;
        }

        /// <summary>
        /// TcpClientWrapper around the tcp client with retry functionality
        /// </summary>
        /// <param name="tcpClientProperties"></param>
        public TcpClientWrapper(TcpClientProperties tcpClientProperties) : this(tcpClientProperties, new PatchedTcpClient(), new CountBasedRetry(tcpClientProperties.MaxRetries))
        {
        }        

        /// <summary>
        /// Write byteArray to tcpClient
        /// </summary>
        /// <param name="data"></param>
        /// <param name="i"></param>
        /// <param name="length"></param>
        public void Write(byte[] data, int i, int length)
        {
            _tcpClient.Write(data, 0, data.Length);
            _retryStrategy.Reset();
        }

        /// <summary>
        /// ReInitialize tcp client when allowed
        /// </summary>
        /// <returns></returns>
        public bool ReInitialize()
        {
            if (_retryStrategy.RetryAllowed())
            {
                _tcpClient = new PatchedTcpClient();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Connect to endpoint
        /// </summary>
        public void Connect() { _tcpClient.Connect(_ipEndpoint); }

        /// <summary>
        /// Close tcpClient connection
        /// </summary>
        public void Close() { _tcpClient.Close(); }

        /// <summary>
        /// Check if connected
        /// </summary>
        public bool Connected { get { return _tcpClient.Connected; } }
    }
}
