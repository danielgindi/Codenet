using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Codenet.Text
{
    public static class SharpReplacer
    {
        public delegate string SharpCodeValueDelegate(string code);

        /// <summary>
        /// Replaces codes in the strings with the values from the dictionary.
        /// Code may appear in the #CODE# form.
        /// Use double sharp (##) to escape where a literal is required.
        /// Keys that do not appear in the dictionary - will not be replaced
        /// </summary>
        /// <param name="text">The input text</param>
        /// <param name="keyValueMap">Dictionary of the code->value mappings</param>
        /// <param name="preserveNotFoundValues">Dictionary of the code->value mappings</param>
        /// <returns>Processed data after replacing the sharps with the corresponding values</returns>
        public static string ReplaceSharps(string input, Dictionary<string, string> keyValueMap, bool preserveNotFoundValues = true)
        {
            if (input == null || input.Length == 0) return input;

            return ReplaceSharps(input, key => { string value = null; keyValueMap.TryGetValue(key, out value); return value; }, preserveNotFoundValues);
        }

        /// <summary>
        /// Replaces codes in the strings with the values from the dictionary.
        /// Code may appear in the #CODE# form.
        /// Use double sharp (##) to escape where a literal is required.
        /// Keys that do not appear in the dictionary - will not be replaced
        /// </summary>
        /// <param name="text">The input text</param>
        /// <param name="supplier">Supplier of values for specific codes</param>
        /// <param name="preserveNotFoundValues">Dictionary of the code->value mappings</param>
        /// <returns>Processed data after replacing the sharps with the corresponding values</returns>
        public static string ReplaceSharps(string input, SharpCodeValueDelegate supplier, bool preserveNotFoundValues = true)
        {
            if (input == null || input.Length == 0) return input;

            var sb = new StringBuilder();
            int firstSharp = -1;

            char c;
            for (int j = 0; j < input.Length; j++)
            {
                c = input[j];
                if (c == '#')
                {
                    if (firstSharp == -1)
                    {
                        firstSharp = j;
                    }
                    else if (firstSharp == j - 1)
                    { // Convert ## to #, to allow escaping those #
                        firstSharp = -1;
                        sb.Append(c);
                    }
                    else
                    {
                        string value = null;
                        if (j - firstSharp > 1)
                        {
                            if (supplier != null)
                            {
                                value = supplier(input.Substring(firstSharp + 1, j - firstSharp - 1));
                            }
                            if (value != null)
                            {
                                sb.Append(value);
                            }
                        }
                        if (value != null || !preserveNotFoundValues)
                        {
                            firstSharp = -1;
                        }
                        else
                        {
                            sb.Append(input.Substring(firstSharp, j - firstSharp));
                            firstSharp = j;
                        }
                    }
                }
                else if (firstSharp == -1)
                {
                    sb.Append(c);
                }
            }
            if (firstSharp > -1)
            {
                sb.Append(input.Substring(firstSharp));
            }

            return sb.ToString();
        }
    }
}
