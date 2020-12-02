namespace ProxyMediator.Core.Misc
{
    public class Address
    {
        public Address(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }

        public string UserName { get; set; }
        public string Password { get; set; }

        public string Hostname { get; }
        public int Port { get; }

        private bool Equals(Address other)
        {
            return string.Equals(Hostname, other.Hostname) && Port == other.Port;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Address) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Hostname?.GetHashCode() ?? 0) * 397) ^ Port;
            }
        }
    }
}