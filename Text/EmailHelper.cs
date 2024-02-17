using System.Text.RegularExpressions;

namespace Codenet.Text;

public static class EmailHelper
{
    public static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[A-Z0-9\._%+\-]+@[A-Z0-9\-]+(\.[A-Z0-9\-]+)*$", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
    }

    /// <summary>
    /// Normalizes an email
    /// Lowers the case
    /// Considers gmail "dot" and "plus" tricks
    /// </summary>
    /// <param name="email">input email address</param>
    /// <returns>normalized email address, or null if invalid</returns>
    public static string NormalizeEmail(string email)
    {
        email = email.Trim().ToLowerInvariant();
        if (!IsValidEmail(email)) return null;
        int idx = email.IndexOf(@"@");
        string un = email.Substring(0, idx);
        string domain = email.Substring(idx);
        if (domain == @"@gmail.com")
        {
            un = un.Replace(@".", "");
            idx = un.IndexOf(@"+");
            if (idx >= 0) un = un.Substring(0, idx);
        }
        return un + domain;
    }

    /// <summary>
    /// Extracts the domain name out of an email address
    /// </summary>
    /// <param name="email"></param>
    /// <returns>Domain name or null</returns>
    public static string GetEmailDomain(string email)
    {
        int atIndex = email.IndexOf('@');
        if (atIndex > -1)
        {
            return email.Remove(0, atIndex + 1);
        }
        else
        {
            return null;
        }
    }
}
