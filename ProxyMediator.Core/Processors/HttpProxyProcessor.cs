using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ProxyMediator.Core.Enums;
using ProxyMediator.Core.Headers;
using ProxyMediator.Core.Misc;
using ProxyMediator.Core.Sessions;
using ProxyMediator.Core.Tunnels;

namespace ProxyMediator.Core.Processors
{
    public class HttpProxyProcessor : IProcessor
    {
        private const int BufferSize = 8192;
        private static readonly HttpProxyProcessor Self = new HttpProxyProcessor();

        private HttpProxyProcessor()
        {
        }

        public async Task<LastPipeState> Run(ProxyMediatorHandler handler, SessionContext context)
        {
            if (IsNewHostRequired(context))
            {
                return LastPipeState.NewHostConnectionRequired;
            }

            using (OneWayTunnel(context.HostStream, context.ClientStream))
            {
                var buffer = new byte[BufferSize];

                try
                {
                    int bytesRead;
                    do
                    {
                        context.Header = await GetHeader(context.Header, context.ClientStream);

                        if (context.Header == null)
                        {
                            return LastPipeState.GameOver;
                        }

                        if (IsNewHostRequired(context))
                        {
                            return LastPipeState.NewHostConnectionRequired;
                        }
                        var header = context.Header;
                        if (context.CurrentExternalProxy != null)
                        {
                            header.AppendContextLine(context.CurrentExternalProxy.AuthHeaderLine);
                        }
                        bytesRead = await ForwardHeader(header, context.HostStream);
                        if (HasBody(header))
                        {
                            bytesRead = await ForwardBody(context.ClientStream, context.HostStream, context.Header.ContentLength, buffer);
                        }
                        context.Header = null;
                    } while (bytesRead > 0);
                }
                catch (ObjectDisposedException)
                {
                    return LastPipeState.NewHostConnectionRequired;
                }
            }

            return LastPipeState.GameOver;
        }

        public static HttpProxyProcessor Instance()
        {
            return Self;
        }

        private static bool IsNewHostRequired(SessionContext sessionContext)
        {
            return sessionContext.CurrentHostAddress == null || !Equals(sessionContext.Header.Host, sessionContext.CurrentHostAddress);
        }

        private static TcpOneWayTunnel OneWayTunnel(NetworkStream source, NetworkStream destination)
        {
            var tunnel = new TcpOneWayTunnel();
            tunnel.Run(destination, source);
            return tunnel;
        }

        private static async Task<HttpHeader> GetHeader(HttpHeader header, Stream stream)
        {
            return header ?? await HttpHeaderStream.GetHeader(stream);
        }

        internal static async Task<int> ForwardHeader(HttpHeader httpHeader, Stream host)
        {
            await host.WriteAsync(httpHeader.Array, 0, httpHeader.Array.Length);
            return httpHeader.Array.Length;
        }

        private static bool HasBody(HttpHeader header)
        {
            return header.ContentLength > 0;
        }

        private static async Task<int> ForwardBody(Stream client, Stream host, long contentLength, byte[] buffer)
        {
            int bytesRead;

            do
            {
                bytesRead = await client.ReadAsync(buffer, 0, contentLength > BufferSize ? BufferSize : (int)contentLength);
                await host.WriteAsync(buffer, 0, bytesRead);
                contentLength -= bytesRead;
            } while (bytesRead > 0 && contentLength > 0);

            return bytesRead;
        }
    }
}
