namespace ProxyMediator.Core.Enums
{
    public enum LastPipeState
    {
        GameStart = 0,
        Initialized = 1,
        NewHostConnectionRequired = 2,
        NewHostConnected = 3,
        PreConnectFinish = 4,
        HttpProxyRequired = 5,
        HttpsTunnelRequired = 6,
        GameOver = 7
    }
}