using System.Threading.Tasks;
using ProxyMediator.Core.Enums;
using ProxyMediator.Core.Headers;
using ProxyMediator.Core.Misc;
using ProxyMediator.Core.Sessions;

namespace ProxyMediator.Core.Processors
{
    public class SetupProcessor : IProcessor
    {
        private static readonly SetupProcessor Self = new SetupProcessor();

        private SetupProcessor()
        {
        }

        public async Task<LastPipeState> Run(ProxyMediatorHandler handler, SessionContext context)
        {
            context.Header = await HttpHeaderStream.GetHeader(context.ClientStream);

            return context.Header != null ? LastPipeState.Initialized : LastPipeState.GameOver;
        }

        public static SetupProcessor Instance()
        {
            return Self;
        }
    }
}