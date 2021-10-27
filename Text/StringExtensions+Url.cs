namespace Codenet.Text
{
    public static partial class StringExtensions
    {
        public static string UrlEncode(this string input)
        {
            return System.Net.WebUtility.UrlEncode(input);
        }
    }
}
