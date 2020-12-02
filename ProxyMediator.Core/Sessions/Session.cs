using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using ProxyMediator.Core.Configurations;
using ProxyMediator.Core.Enums;
using ProxyMediator.Core.Misc;
using ProxyMediator.Core.Processors;

namespace ProxyMediator.Core.Sessions
{
    public static class Session
    {
        /// <summary>
        /// GameStart->Initialized->[HttpsTunnelRequired|HttpProxyRequired]->NewHostConnectionRequired->NewHostConnected
        /// 
        /// </summary>
        private static readonly Dictionary<LastPipeState, IProcessor> Handlers =
            new Dictionary<LastPipeState, IProcessor>
            {
                {LastPipeState.GameStart, SetupProcessor.Instance()},
                {LastPipeState.Initialized, ProxyTypeProcessor.Instance()},
                {LastPipeState.HttpProxyRequired, HttpProxyProcessor.Instance()},
                {LastPipeState.HttpsTunnelRequired, HttpsTunnelProcessor.Instance()},
                {LastPipeState.NewHostConnectionRequired, NewHostProcessor.Instance()},
                {LastPipeState.NewHostConnected, ProxyTypeProcessor.Instance()}
            };

        public static async Task Run(SessionContextPool pool, TcpClient client, ProxyServerConfig proxyConfig)
        {
            var result = LastPipeState.GameStart;
            string contextId;
            using (var context = new SessionContext(client, proxyConfig))
            {
                contextId = context.Id;
                pool.ContextContainer.TryAdd(contextId, context);
                do
                {
                    try
                    {
                        var processor = Handlers[result];
                        //Console.WriteLine($"[{context.Id}] {result} → {processor.GetType()} ↓");
                        result = await processor.Run(proxyConfig.Handler, context);
                    }
                    catch (Exception)
                    {
                        result = LastPipeState.GameOver;
                    }
                } while (result != LastPipeState.GameOver);

                //Console.WriteLine($"[{context.Id}] {result} ⬆");
            }

            pool.ContextContainer.TryRemove(contextId, out _);
        }
    }
}