namespace Codenet.Text
{
    public static partial class StringExtensions
    {
        public static bool IsValidEmail(this string input)
        {
            return EmailHelper.IsValidEmail(input);
        }

        /// <summary>
        /// Normalizes an email
        /// Lowers the case
        /// Considers gmail "dot" and "plus" tricks
        /// </summary>
        /// <param name="input">input email address</param>
        /// <returns>normalized email address, or null if invalid</returns>
        public static string NormalizeEmail(this string input)
        {
            return EmailHelper.NormalizeEmail(input);
        }

        /// <summary>
        /// Extracts the domain name out of an email address
        /// </summary>
        /// <returns>Domain name or null</returns>
        public static string GetEmailDomain(this string input)
        {
            return EmailHelper.GetEmailDomain(input);
        }
    }
}
