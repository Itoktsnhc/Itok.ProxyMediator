using ProxyMediator.Core.Sessions;
using System;
using System.Threading.Tasks;

namespace ProxyMediator.Core.Misc
{

    public class StageFunc
    {
        public Func<SessionContext, ProxyMediatorHandler, Task<ExternalProxy>> OnBeforeConnect { get; set; }
    }
}
