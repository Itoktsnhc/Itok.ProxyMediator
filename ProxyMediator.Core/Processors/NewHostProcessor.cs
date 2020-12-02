using System.Net.Sockets;
using System.Threading.Tasks;
using ProxyMediator.Core.Enums;
using ProxyMediator.Core.Misc;
using ProxyMediator.Core.Sessions;

namespace ProxyMediator.Core.Processors
{
    public class NewHostProcessor : IProcessor
    {
        private static readonly NewHostProcessor Self = new NewHostProcessor();

        private NewHostProcessor()
        {
        }

        public async Task<LastPipeState> Run(ProxyMediatorHandler handler, SessionContext context)
        {
            context.RemoveHost();
            if (context.StageFunc?.OnBeforeConnect != null)
            {
                context.CurrentExternalProxy = await context.StageFunc.OnBeforeConnect(context, handler);
            }
            if (context.CurrentExternalProxy == null)
            {
                context.AddHost(await Connect(context.Header.Host.Hostname, context.Header.Host.Port));
            }
            else
            {
                context.AddHost(await Connect(context.CurrentExternalProxy.Address, context.CurrentExternalProxy.Port));
            }

            context.CurrentHostAddress = context.Header.Host;

            return LastPipeState.NewHostConnected;
        }

        public static NewHostProcessor Instance()
        {
            return Self;
        }

        private static async Task<TcpClient> Connect(string hostname, int port)
        {
            var host = new TcpClient();
            await host.ConnectAsync(hostname, port);
            return host;
        }
    }
}
