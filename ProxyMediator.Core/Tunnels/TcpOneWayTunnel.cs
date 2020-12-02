using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyMediator.Core.Tunnels
{
    public sealed class TcpOneWayTunnel : IDisposable
    {
        private const int BufferSize = 8192;
        private CancellationTokenSource _cancellationTokenSource;
        private NetworkStream _destStream;
        private NetworkStream _sourceStream;

        public TcpOneWayTunnel()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public async void Run(NetworkStream destination, NetworkStream source)
        {
            _destStream = destination;
            _sourceStream = source;
            await Task.WhenAny(
                Tunnel(_sourceStream, _destStream, _cancellationTokenSource.Token));
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
            finally
            {
                source.Dispose();
            }
        }
    }
}
