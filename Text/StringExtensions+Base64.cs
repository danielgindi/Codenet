using System;
using System.Text;

namespace Codenet.Text
{
    public static partial class StringExtensions
    {
        public static string EncodeToBase64String(this string value, bool lineBreaks = false)
        {
            return Convert.ToBase64String(
                UTF8Encoding.UTF8.GetBytes(value),
                lineBreaks ? Base64FormattingOptions.InsertLineBreaks : Base64FormattingOptions.None
            );
        }

        public static string DecodeFromBase64(this string value)
        {
            return UTF8Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
    }
}
