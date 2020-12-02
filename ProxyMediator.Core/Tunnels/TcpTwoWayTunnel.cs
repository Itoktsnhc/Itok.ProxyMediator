using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyMediator.Core.Tunnels
{
    public sealed class TcpTwoWayTunnel : IDisposable
    {
        private const int BufferSize = 8192;
        private CancellationTokenSource _cancellationTokenSource;

        public TcpTwoWayTunnel()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task Run(NetworkStream client, NetworkStream host)
        {
            await Task.WhenAny(
                Tunnel(client, host, _cancellationTokenSource.Token),
                Tunnel(host, client, _cancellationTokenSource.Token));
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_cancellationTokenSource == null) return;
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        private static async Task Tunnel(Stream source, Stream destination, CancellationToken token)
        {
            var buffer = new byte[BufferSize];

            try
            {
                int bytesRead;
                do
                {
                    bytesRead = await source.ReadAsync(buffer, 0, BufferSize, token);
                    await destination.WriteAsync(buffer, 0, bytesRead, token);
                } while (bytesRead > 0 && !token.IsCancellationRequested);
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}