using System;
using System.Net.Sockets;
using ProxyMediator.Core.Configurations;
using ProxyMediator.Core.Headers;
using ProxyMediator.Core.Misc;

namespace ProxyMediator.Core.Sessions
{
    public sealed class SessionContext : IDisposable
    {
        public readonly string Id;

        public SessionContext(TcpClient client, ProxyServerConfig proxyConfig)
        {
            Id = Guid.NewGuid().ToString("N");
            ProxyConfig = proxyConfig;
            StageFunc = proxyConfig.StateFunc;
            Client = client;
            ClientStream = client.GetStream();
        }

        public ProxyServerConfig ProxyConfig { get; }

        public HttpHeader Header { get; set; }

        public Address CurrentHostAddress { get; set; }

        public TcpClient Client { get; internal set; }

        public NetworkStream ClientStream { get; internal set; }

        public TcpClient Host { get; private set; }

        public NetworkStream HostStream { get; internal set; }

        public void Dispose()
        {
            Dispose(true);
        }

        public void AddHost(TcpClient client)
        {
            Host = client;
            HostStream = client.GetStream();
        }

        public void RemoveHost()
        {
            using (HostStream)
            using (Host)
            {
            }

            HostStream = null;
            Host = null;
            CurrentExternalProxy = null;
        }

        public StageFunc StageFunc { get; set; }

        public ExternalProxy CurrentExternalProxy { get; set; }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ClientStream != null)
                {
                    using (ClientStream)
                    {
                    }

                    ClientStream = null;
                }

                if (Client != null)
                {
                    using (Client)
                    {
                    }

                    Client = null;
                }

                if (HostStream != null)
                {
                    using (HostStream)
                    {
                    }

                    HostStream = null;
                }

                if (Host != null)
                {
                    using (Host)
                    {
                    }

                    Host = null;
                }
            }
        }
    }
}