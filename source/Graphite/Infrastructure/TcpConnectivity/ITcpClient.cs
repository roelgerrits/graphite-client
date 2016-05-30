using System.Net;
using System.Net.Sockets;

namespace Graphite.Infrastructure.TcpConnectivity
{
    /// <summary>
    /// IPatchedTcpClient
    /// </summary>
    public interface IPatchedTcpClient
    {
        /// <summary>
        /// Close tcp client
        /// </summary>
        void Close();
        /// <summary>
        /// Connected
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// Connect to endpoint
        /// </summary>
        /// <param name="ipEndpoint"></param>
        void Connect(IPEndPoint ipEndpoint);
        /// <summary>
        /// Write to tcp connection
        /// </summary>
        /// <param name="data"></param>
        /// <param name="i"></param>
        /// <param name="length"></param>
        void Write(byte[] data, int i, int length);
    }

    /// <summary>
    /// PatchedTcpClient is wrapping the tcpclient object by implementing the ITcpClient inteface
    /// </summary>
    public class PatchedTcpClient : TcpClient, IPatchedTcpClient
    {
        /// <summary>
        /// PatchedTcpClient
        /// </summary>
        public PatchedTcpClient()
        {
            ExclusiveAddressUse = false;
        }
        /// <summary>
        /// Write to tcp connection
        /// </summary>
        /// <param name="data"></param>
        /// <param name="i"></param>
        /// <param name="length"></param>
        public void Write(byte[] data, int i, int length)
        {
            GetStream().Write(data, i, length);
        }
    }
}