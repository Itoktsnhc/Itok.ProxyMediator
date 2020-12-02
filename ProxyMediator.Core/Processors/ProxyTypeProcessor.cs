using System.Threading.Tasks;
using ProxyMediator.Core.Enums;
using ProxyMediator.Core.Misc;
using ProxyMediator.Core.Sessions;

namespace ProxyMediator.Core.Processors
{
    public class ProxyTypeProcessor : IProcessor
    {
        private static readonly ProxyTypeProcessor Self = new ProxyTypeProcessor();

        private ProxyTypeProcessor()
        {
        }

        public Task<LastPipeState> Run(ProxyMediatorHandler handler, SessionContext context)
        {
            return Task.FromResult(context.Header.Verb == "CONNECT"
                ? LastPipeState.HttpsTunnelRequired
                : LastPipeState.HttpProxyRequired);
        }

        public static ProxyTypeProcessor Instance()
        {
            return Self;
        }
    }
}