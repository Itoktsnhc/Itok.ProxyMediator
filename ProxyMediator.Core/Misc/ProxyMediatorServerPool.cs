using System.Collections.Concurrent;
using System.Net;

namespace ProxyMediator.Core.Misc
{
    public class ProxyMediatorHandler
    {
        public IPEndPoint EndPoint { get; }
        public ConcurrentDictionary<string, ExternalProxy> OutBoundMap { get; }
        public SessionContextPool Pool { get; }

        public ProxyMediatorHandler(SessionContextPool pool, IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            OutBoundMap = new ConcurrentDictionary<string, ExternalProxy>();
            Pool = pool;
        }
    }
}