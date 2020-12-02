using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProxyMediator.Core.Misc;
using ProxyMediator.Extension;

namespace ProxyMediator
{
    static class Program
    {
        static void Main()
        {
            var ext = new HostBuilder().ConfigureServices(svc =>
                {
                    svc.AddProxyMediator(IPEndPoint.Parse("127.0.0.1:0"), (session, proxyMediatorHandler) =>
                    {
                        var host = session.Header.Host.Hostname;
                        var external = proxyMediatorHandler.OutBoundMap.OrderByDescending(s => s.Key.Length)
                            .FirstOrDefault(s =>
                                host.Contains(s.Key)
                                || host == s.Key).Value;
                        return Task.FromResult(external);
                    });
                    svc.AddHttpClient(nameof(ProxyMediator)).ConfigurePrimaryHttpMessageHandler(ctx =>
                    {
                        var proxyMediatorHandler = ctx.GetRequiredService<ProxyMediatorHandler>();
                        var httpClientHandler = new HttpClientHandler();
                        while (proxyMediatorHandler.EndPoint.Port == 0)
                        {
                            Thread.Sleep(100);
                        }

                        httpClientHandler.UseProxy = true;
                        httpClientHandler.Proxy = new WebProxy(proxyMediatorHandler.EndPoint.ToString());
                        return httpClientHandler;
                    });
                }
            ).Build();
            ext.Start();
            var factory = ext.Services.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient(nameof(ProxyMediator));
            var handler = ext.Services.GetRequiredService<ProxyMediatorHandler>();
            foreach (var unused in Enumerable.Range(1, 3))
            {
                client.GetStringAsync("https://ip.42.pl/raw").Wait();
            }

            foreach (var item in handler.Pool.ContextContainer.Values)
            {
                item.Dispose();
            }

            Console.ReadLine();
            var ds = client.GetStringAsync("http://ip.42.pl/raw").Result;
            Console.WriteLine(ds);
            ds = client.GetStringAsync("http://ip.42.pl/raw").Result;
            Console.WriteLine(ds);
        }
    }
}