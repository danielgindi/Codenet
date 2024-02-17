namespace Codenet.Text;

public static partial class StringExtensions
{
    public static string HtmlEncode(this string input)
    {
        return System.Net.WebUtility.HtmlEncode(input);
    }
}
