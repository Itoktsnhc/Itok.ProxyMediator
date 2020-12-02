using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProxyMediator.Core;
using ProxyMediator.Core.Configurations;
using ProxyMediator.Core.Misc;

namespace ProxyMediator.Extension
{
    public class ProxyMediatorBackgroundService : CoordinatedBackgroundService
    {
        private readonly ProxyMediatorHandler _handler;
        private readonly ILogger<ProxyMediatorBackgroundService> _logger;
        private readonly StageFunc _onStageFunc;

        public ProxyMediatorBackgroundService(ProxyMediatorHandler handler,
            ILogger<ProxyMediatorBackgroundService> logger,
            StageFunc onStageFunc,
            IHostApplicationLifetime appLifetime) :
            base(appLifetime)
        {
            _handler = handler;
            _logger = logger;
            _onStageFunc = onStageFunc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ProxyMediatorServer proxyMediatorServer = null;
                try
                {
                    var proxyMediatorServerStop =
                        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    var proxyServerConfig = new ProxyServerConfig(_handler)
                    {
                        StateFunc = _onStageFunc,
                    };
                    proxyMediatorServer = new ProxyMediatorServer(proxyServerConfig);
                    stoppingToken.Register(() =>
                    {
                        try
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            proxyMediatorServer.Dispose();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"error when disposing {nameof(ProxyMediatorServer)}");
                        }

                        proxyMediatorServerStop.SetResult(true);
                    });
                    await proxyMediatorServerStop.Task;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"error in {nameof(ProxyMediatorBackgroundService)}");
                }
                finally
                {
                    proxyMediatorServer?.Dispose();
                }
            }
        }
    }

}