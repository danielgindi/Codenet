using System.Text.RegularExpressions;

namespace Codenet.Text;

public static class HtmlHelper
{
    public static string StripHtml(string content)
    {
        //Strips the <script> tags from the Html
        content = Regex.Replace(content,
            @"<script[^>.]*>[\s\S]*?</script>", " ",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        //Strips the <style> tags from the Html
        content = Regex.Replace(content,
            @"<style[^>.]*>[\s\S]*?</style>", " ",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        //Strips the <!--comment--> tags from the Html	
        content = Regex.Replace(content,
            @"<!(?:--[\s\S]*?--\s*)?>", " ",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        //Strips the HTML tags from the Html
        content = Regex.Replace(content, @"<(.|\n)+?>", " ", RegexOptions.IgnoreCase);

        //Decode all html entities
        content = System.Net.WebUtility.HtmlDecode(content);

        // remove all double spacing
        content = Regex.Replace(content, @"[ \t\r\n\u00a0]+", @" ");

        return content;
    }
}
