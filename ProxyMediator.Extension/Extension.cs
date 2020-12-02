using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ProxyMediator.Core.Misc;
using ProxyMediator.Core.Sessions;

namespace ProxyMediator.Extension
{
    public static class ProxyMediatorExtension
    {
        public static IServiceCollection AddProxyMediator(this IServiceCollection svc,
            IPEndPoint ipEndPoint,
            Func<SessionContext, ProxyMediatorHandler, Task<ExternalProxy>> onBeforeConnect
            )
        {
            var handler = new ProxyMediatorHandler(new SessionContextPool(), ipEndPoint);
            svc.AddSingleton(_ => handler);
            svc.AddSingleton(_ => new StageFunc()
            {
                OnBeforeConnect = onBeforeConnect
            });
            svc.AddHostedService<ProxyMediatorBackgroundService>();
            return svc;
        }
    }
}