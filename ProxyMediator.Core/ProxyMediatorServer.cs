using System;
using System.Net.Sockets;
using System.Threading;
using ProxyMediator.Core.Configurations;
using ProxyMediator.Core.Misc;
using ProxyMediator.Core.Sessions;

namespace ProxyMediator.Core
{
    public class ProxyMediatorServer : IDisposable
    {
        private ProxyListener _proxyListener;

        public ProxyMediatorServer(ProxyServerConfig config)
        {
            async void HandleClient(SessionContextPool pool, TcpClient client, CancellationToken _) =>
                await Session.Run(pool, client, config);

            _proxyListener = new ProxyListener(config.Pool, config.EndPoint, HandleClient);
            // ReSharper disable once PossibleNullReferenceException
            var ipPortParts = _proxyListener.InternalTcpListener.LocalEndpoint.ToString().Split(':');
            config.EndPoint.Port = int.Parse(ipPortParts[1]);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && _proxyListener != null)
            {
                _proxyListener.Dispose();
                _proxyListener = null;
            }
        }
    }
}