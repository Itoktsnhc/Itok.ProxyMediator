using System.Threading.Tasks;
using ProxyMediator.Core.Enums;
using ProxyMediator.Core.Misc;
using ProxyMediator.Core.Sessions;

namespace ProxyMediator.Core.Processors
{
    interface IProcessor
    {
        Task<LastPipeState> Run(ProxyMediatorHandler handler, SessionContext context);
    }
}
