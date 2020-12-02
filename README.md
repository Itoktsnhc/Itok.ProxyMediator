# Itok.ProxyMediator


[![Nuget](https://img.shields.io/nuget/dt/itok.proxymediator.Core?label=Itok.ProxyMediator.Core&logo=Nuget)](https://www.nuget.org/packages/Itok.ProxyMediator.Core/)  


Nuget:Itok.ProxyMediator.Core + Itok.ProxyMediator.Extension



一个本地的可配置的切换代理服务，将本地请求按需转发到对于的代理上。Inspired by https://github.com/agabani/PassThroughProxy

UserRequest -----> ProxyMediator ----->[ExternalProxy1,ExternalProxy2] 

示例代码：
``` CSharp

var ext = new HostBuilder().ConfigureServices(svc =>
    {
        svc.AddProxyMediator(IPEndPoint.Parse("127.0.0.1:0"),// 端口为0，让系统自动分配一个空闲端口
        (session, proxyMediatorHandler) =>//当连接到远端之前，尝试获取目标转发代理
        {
            var host = session.Header.Host.Hostname;
            var external = proxyMediatorHandler.OutBoundMap.OrderByDescending(s => s.Key.Length)//
                .FirstOrDefault(s =>
                    host.Contains(s.Key)
                    || host == s.Key).Value;
            return Task.FromResult(external);
        });
        svc.AddHttpClient(nameof(ProxyMediator))//注册HttpClient，并配置代理为本地服务。
        .ConfigurePrimaryHttpMessageHandler(ctx =>
        {
            var proxyMediatorHandler = ctx.GetRequiredService<ProxyMediatorHandler>();
            var httpClientHandler = new HttpClientHandler();
            while (proxyMediatorHandler.EndPoint.Port == 0)//当系统自动分配的时候，需要等待后台服务启动
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

foreach (var item in handler.Pool.ContextContainer.Values)//清空现有的两端链接
{
    item.Dispose();
}

Console.ReadLine();
var ds = client.GetStringAsync("http://ip.42.pl/raw").Result;
Console.WriteLine(ds);
ds = client.GetStringAsync("http://ip.42.pl/raw").Result;
Console.WriteLine(ds);
```

