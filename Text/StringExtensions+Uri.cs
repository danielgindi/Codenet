using System;
using System.Collections.Specialized;

namespace Codenet.Text
{
    public static partial class StringExtensions
    {
        public static NameValueCollection ParseQueryString(this string qs)
        {
            var nvc = new NameValueCollection();

            foreach (var param in qs.Split('&'))
            {
                var i = param.IndexOf('=');
                var key = i == -1 ? param : param.Substring(0, i);
                var value = i == -1 ? null : param.Remove(0, i + 1);
                nvc.Add(Uri.UnescapeDataString(key), value == null ? null : Uri.UnescapeDataString(value));
            }

            return nvc;
        }
    }
}
