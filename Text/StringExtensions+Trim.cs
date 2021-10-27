namespace Codenet.Text
{
    public static partial class StringExtensions
    {
        public static string TrimToNull(this string value)
        {
            if (value == null) return null;

            value = value.Trim();

            if (value.Length == 0)
                return null;

            return value;
        }

        public static string TrimToEmailOrNull(this string value)
        {
            if (value == null) return null;

            value = value.Trim();

            if (value.Length == 0 || !EmailHelper.IsValidEmail(value))
                return null;

            return value;
        }
    }
}
