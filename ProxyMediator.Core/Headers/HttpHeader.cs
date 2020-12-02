using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyMediator.Core.Misc;

namespace ProxyMediator.Core.Headers
{
    public class HttpHeader
    {
        public HttpHeader(byte[] array, bool ignoreProxyAuthHeader = true)
        {
            Parse(this, array, ignoreProxyAuthHeader);
        }

        public Address Host { get; set; }
        public long ContentLength { get; private set; }
        public string Verb { get; private set; }
        public byte[] Array { get; private set; }
        private IEnumerable<string> ArrayList { get; set; }

        public void AppendContextLine(string headerLine)
        {
            Parse(this, ArrayList.Concat(new[] {headerLine}).ToArray(), false);
        }

        private static void Parse(HttpHeader self, byte[] array, bool ignoreProxyAuthHeader = true)
        {
            var strings = Encoding.ASCII.GetString(array).Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            Parse(self, strings, ignoreProxyAuthHeader);
        }

        private static void Parse(HttpHeader self, string[] strings, bool ignoreProxyAuthHeader = true)
        {
            self.Host = GetAddress(strings);
            self.ContentLength = GetContentLength(strings);
            self.Verb = GetVerb(strings);
            self.Array = GetArray(strings, ignoreProxyAuthHeader);
            self.ArrayList = strings;
        }

        private static byte[] GetArray(IEnumerable<string> arrayList, bool ignoreProxyAuthHeader = true)
        {
            var builder = new StringBuilder();
            var enumerable = arrayList.AsEnumerable();
            if (ignoreProxyAuthHeader)
            {
                enumerable = enumerable
                    .Where(@string =>
                        !@string.StartsWith("Proxy-Authorization:", StringComparison.OrdinalIgnoreCase));
            }

            foreach (var @string in enumerable)
            {
                builder.Append(@string).Append("\r\n");
            }

            builder.Append("\r\n");

            return Encoding.ASCII.GetBytes(builder.ToString());
        }

        private static Address GetAddress(IEnumerable<string> strings)
        {
            const string key = "host:";

            var hostParts = strings.Where(s => !string.IsNullOrEmpty(s))
                .Single(s => s.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                .Substring(key.Length)
                .TrimStart()
                .Split(':');

            switch (hostParts.Length)
            {
                case 1:
                    return new Address(hostParts[0], 80);
                case 2:
                    return new Address(hostParts[0], int.Parse(hostParts[1]));
                default:
                    throw new FormatException(string.Join(":", hostParts));
            }
        }

        private static long GetContentLength(IEnumerable<string> strings)
        {
            const string key = "content-length:";

            return Convert.ToInt64(strings.Where(s => !string.IsNullOrEmpty(s))
                .SingleOrDefault(s => s.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                ?.Substring(key.Length)
                .TrimStart());
        }

        private static string GetVerb(IEnumerable<string> strings)
        {
            return strings.First().Split(' ')[0];
        }
    }
}