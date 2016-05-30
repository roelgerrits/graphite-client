using System.Net;

namespace Graphite.Infrastructure.TcpConnectivity
{
    public class TcpClientProperties
    {
        /// <summary>
        /// TcpClientProperties is a container class for tcp client properties
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="maxRetries"></param>
        public TcpClientProperties(IPAddress address, int port, int maxRetries) {
            Address = address;
            Port = port;
            MaxRetries = maxRetries;
        }

        /// <summary>
        /// IPAddress for tcp client
        /// </summary>
        public IPAddress Address { get; set; }
        /// <summary>
        /// Port for tcp client
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// MaxRetries after failure
        /// </summary>
        public int MaxRetries { get; set; }
    }
}
