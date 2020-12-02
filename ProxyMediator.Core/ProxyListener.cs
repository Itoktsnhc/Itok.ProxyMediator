using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ProxyMediator.Core.Misc;

namespace ProxyMediator.Core
{
    public sealed class ProxyListener : IDisposable
    {
        public TcpListener InternalTcpListener { get; private set; }
        private CancellationTokenSource _source;
        private SessionContextPool Pool { get; }

        public ProxyListener(SessionContextPool pool, IPEndPoint endPoint,
            Action<SessionContextPool, TcpClient, CancellationToken> handleClient)
        {
            Pool = pool;
            InternalTcpListener = new TcpListener(endPoint);
            _source = new CancellationTokenSource();
            InternalTcpListener.Start();
            AcceptClients(InternalTcpListener, handleClient, _source.Token);
        }

        private async void AcceptClients(TcpListener listener,
            Action<SessionContextPool, TcpClient, CancellationToken> handleClient,
            CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var tcpClient = await listener.AcceptTcpClientAsync();
                handleClient(Pool, tcpClient, token);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_source != null)
                {
                    _source.Cancel();
                    _source.Dispose();
                    _source = null;
                }

                if (InternalTcpListener != null)
                {
                    InternalTcpListener.Stop();
                    InternalTcpListener = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}