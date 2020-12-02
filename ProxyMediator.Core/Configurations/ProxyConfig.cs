using System.Net;
using ProxyMediator.Core.Misc;

namespace ProxyMediator.Core.Configurations
{
    public class ProxyServerConfig
    {
        public ProxyServerConfig(ProxyMediatorHandler handler)
        {
            Handler = handler;
        }

        public ProxyMediatorHandler Handler { get; }

        public IPEndPoint EndPoint => Handler.EndPoint;
        public SessionContextPool Pool => Handler.Pool;
        public StageFunc StateFunc { get; set; }
    }
}