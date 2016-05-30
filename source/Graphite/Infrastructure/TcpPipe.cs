using Graphite.Infrastructure.TcpConnectivity;
using System;
using System.Net;

namespace Graphite.Infrastructure
{
    internal class TcpPipe : IPipe, IDisposable
    {
        private readonly TcpSenderPipe sender;

        private bool disposed;

        public TcpPipe(TcpClientProperties tcpClientProperties)
        {
            sender = new TcpSenderPipe(tcpClientProperties);
            sender.Run();
        }

        public bool Send(string message)
        {
            return this.sender.Send(message);
        }

        public bool Send(string[] messages)
        {
            return this.sender.Send(messages);
        }

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                try
                {
                    if (this.sender != null)
                    {
                        this.sender.Dispose();
                    }
                }
                catch
                {
                }

                this.disposed = true;
            }
        }
    }
}