using System.Collections.Concurrent;
using ProxyMediator.Core.Sessions;

namespace ProxyMediator.Core.Misc
{
    public class SessionContextPool
    {
        public ConcurrentDictionary<string, SessionContext> ContextContainer { get; set; } =
            new ConcurrentDictionary<string, SessionContext>();
    }
}