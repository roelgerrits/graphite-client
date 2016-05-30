using Graphite.Infrastructure.TcpConnectivity;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Graphite.Infrastructure
{
    public class TcpSenderPipe : IPipe, IDisposable
    {
        //private readonly IPEndPoint endpoint;

        private ITcpClientWrapper tcpClientWrapper;

        private bool disposed;

        private readonly BlockingCollection<string> messageList = new BlockingCollection<string>();

        public CancellationTokenSource TokenSource { get; }

        public TcpSenderPipe(ITcpClientWrapper tcpClientWrapper)
        {
            TokenSource = new CancellationTokenSource();
            this.tcpClientWrapper = tcpClientWrapper;
        }
        public TcpSenderPipe(TcpClientProperties tcpClientProperties) : this(new TcpClientWrapper(tcpClientProperties))
        {
        }

        public Task Run()
        {
            var task = Task.Factory.StartNew(this.ProcessMessages, TaskCreationOptions.LongRunning);

            // Handle exceptions
            task.ContinueWith(
                t =>
                {
                    if (t.Exception != null)
                    {
                        foreach (var exception in t.Exception.InnerExceptions.Where(e => !(e is OperationCanceledException)))
                        {
                            Logging.Source.TraceEvent(TraceEventType.Error, 0, exception.Format());
                        }
                    }
                },
                TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        public bool Send(string message)
        {
            if (message == null)
                return false;

            return this.Send(new[] { message });
        }

        public bool Send(string[] messages)
        {
            if (messages == null)
                return false;

            this.messageList.Add(string.Join("\n", messages), this.TokenSource.Token);

            return true;
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
                    this.TokenSource.Cancel();

                    if (this.tcpClientWrapper != null)
                    {
                        this.tcpClientWrapper.Close();
                    }
                }
                catch
                {
                }

                this.disposed = true;
            }
        }


        private void ProcessMessages()
        {
            do
            {
                try
                {
                    string message = this.messageList.Take(this.TokenSource.Token);
                    var data = Encoding.Default.GetBytes(message + "\n");

                    CoreSend(data);
                }
                catch (Exception ex)
                {
                    // Exception filtering
                    if (ex.GetType().Name != "InvalidOperationException" && ex.GetType().Name != "SocketException")
                    {
                        throw;
                    }

                    // When not recovering from the tcpclient failure, throw exception
                    if (!tcpClientWrapper.ReInitialize())
                    {
                        throw;
                    }
                }
            }
            while (!TokenSource.Token.IsCancellationRequested);
        }

        private void HandleProcessMessageException(Exception ex, ref int retry)
        {
            
        }

        private void EnsureConnected()
        {
            if (tcpClientWrapper.Connected)
                return;

            tcpClientWrapper.Connect();
        }

        private bool CoreSend(byte[] data)
        {
            EnsureConnected();

            try
            {
                tcpClientWrapper.Write(data, 0, data.Length);
                    
                return true;
            }
            catch (IOException exception)
            {
                Logging.Source.TraceEvent(TraceEventType.Error, 0, exception.Format());
            }
            catch (ObjectDisposedException exception)
            {
                Logging.Source.TraceEvent(TraceEventType.Error, 0, exception.Format());
            }

            return false;
        }
    }
}
