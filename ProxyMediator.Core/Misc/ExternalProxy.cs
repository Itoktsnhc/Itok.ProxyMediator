using System;
using System.Text;

namespace ProxyMediator.Core.Misc
{
    public class ExternalProxy
    {
        public ExternalProxy(string address, int port)
        {
            Address = address;
            Port = port;
        }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AuthHeaderLine
        {
            get
            {
                if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                {
                    return null;
                }
                return $"Proxy-Authorization: Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{Password}"))}";
            }
        }
    }
}
