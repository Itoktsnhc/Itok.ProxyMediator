using System.IO;
using System.Text;
using System.Threading.Tasks;
using ProxyMediator.Core.Enums;
using ProxyMediator.Core.Misc;
using ProxyMediator.Core.Sessions;
using ProxyMediator.Core.Tunnels;

namespace ProxyMediator.Core.Processors
{
    public class HttpsTunnelProcessor : IProcessor
    {
        private static readonly HttpsTunnelProcessor Self = new HttpsTunnelProcessor();

        private HttpsTunnelProcessor()
        {
        }

        public async Task<LastPipeState> Run(ProxyMediatorHandler handler, SessionContext context)
        {
            if (context.CurrentHostAddress == null || !Equals(context.Header.Host, context.CurrentHostAddress))
            {
                return LastPipeState.NewHostConnectionRequired;
            }

            var header = context.Header;
            if (context.CurrentExternalProxy != null)
            {
                header.AppendContextLine(context.CurrentExternalProxy.AuthHeaderLine);
                await HttpProxyProcessor.ForwardHeader(header, context.HostStream);
            }

            using var tunnel = new TcpTwoWayTunnel();
            var task = tunnel.Run(context.ClientStream, context.HostStream);
            if (context.CurrentExternalProxy == null)
            {
                await SendConnectionEstablised(context.ClientStream);
            }
            await task;

            return LastPipeState.GameOver;
        }

        public static HttpsTunnelProcessor Instance()
        {
            return Self;
        }
        private static async Task SendConnectionEstablised(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection established\r\n\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}