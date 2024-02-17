using System;

namespace Codenet.Text;

public static class StringHelper
{
    public static string CutByWords(string input, int maxChars, string suffixWhenCut = null)
    {
        string text = input;

        if (maxChars > 0 && text.Length > maxChars)
        {
            for (int i = maxChars; i >= 0; i--)
            {
                if (text[i] == ' ')
                {
                    text = text.Remove(i);
                    break;
                }
            }
        }

        if (suffixWhenCut != null && text.Length != input.Length)
            text += suffixWhenCut;

        return text;
    }

    /// <summary>
    /// Generates a random string out of the supplied character string.
    /// </summary>
    /// <param name="possibleCharacters">Possible characters.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static string GenerateRandomString(string possibleCharacters, int length)
    {
        var randomString = "";
        var rnd = new Random(Guid.NewGuid().GetHashCode());
        while (length-- > 0)
        {
            randomString += possibleCharacters[rnd.Next(possibleCharacters.Length)];
        }
        return randomString;
    }

    /// <summary>
    /// Generates a random string out of the supplied character string.
    /// </summary>
    /// <param name="possibleCharacters">Possible characters.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static string GenerateRandomString(char[] possibleCharacters, int length)
    {
        var randomString = "";
        var rnd = new Random(Guid.NewGuid().GetHashCode());
        while (length-- > 0)
        {
            randomString += possibleCharacters[rnd.Next(possibleCharacters.Length)];
        }
        return randomString;
    }
}
