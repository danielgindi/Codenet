using System;
using System.Collections.Generic;
using System.Text;

namespace Codenet.Text
{
    public static partial class StringExtensions
    {
        public static string[] SplitOrEmptyArray(this string value, params char[] separator)
        {
            if (value.Length == 0)
                return new string[] { };

            return value.Split(separator);
        }

        public static string[] SplitOrEmptyArray(this string value, char[] separator, int count)
        {
            if (value.Length == 0)
                return new string[] { };

            return value.Split(separator, count);
        }

        public static string[] SplitOrEmptyArray(this string value, char[] separator, StringSplitOptions options)
        {
            if (value.Length == 0)
                return new string[] { };

            return value.Split(separator, options);
        }

        public static string[] SplitOrEmptyArray(this string value, string[] separator, StringSplitOptions options)
        {
            if (value.Length == 0)
                return new string[] { };

            return value.Split(separator, options);
        }

        public static string[] SplitOrEmptyArray(this string value, char[] separator, int count, StringSplitOptions options)
        {
            if (value.Length == 0)
                return new string[] { };

            return value.Split(separator, count, options);
        }

        public static string[] SplitOrEmptyArray(this string value, string[] separator, int count, StringSplitOptions options)
        {
            if (value.Length == 0)
                return new string[] { };

            return value.Split(separator, count, options);
        }

        public static Int32[] SplitToInt32(this string value, params char[] delimiters)
        {
            if (value.Length == 0) return new Int32[] { };

            var lst = new List<int>();
            var strs = value.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in strs)
            {
                if (int.TryParse(str, out var i))
                {
                    lst.Add(i);
                }
            }
            return lst.ToArray();
        }

        public static Int64[] SplitToInt64(this string value, params char[] delimiters)
        {
            if (value.Length == 0) return new Int64[] { };

            var lst = new List<Int64>();
            var strs = value.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in strs)
            {
                if (Int64.TryParse(str, out var i))
                {
                    lst.Add(i);
                }
            }
            return lst.ToArray();
        }

        /// <summary>
        /// Split a string, and give also an array of the whitespace that was stripped
        /// </summary>
        /// <returns></returns>
        public static void Split(
            string source, char[] separator,
            out string[] arrStrings, out string[] arrWhitespace)
        {
            if ((source.Length == 0))
            {
                arrStrings = new string[0];
                arrWhitespace = new string[0];
                return;
            }

            int[] sepList = new int[source.Length];
            int numReplaces = MakeSeparatorList(source, separator, ref sepList);
            if (numReplaces == 0)
            {
                arrStrings = new string[] { source };
                arrWhitespace = new string[] { string.Empty, string.Empty };
                return;
            }

            InternalSplitOmitEmptyEntries(source, sepList, null, numReplaces,
                out arrStrings, out arrWhitespace);
        }

        private static int MakeSeparatorList(string source, char[] separator, ref int[] sepList)
        {
            int num = 0;
            int sourceLength = source.Length;
            int length = sepList.Length;
            int separatorCount = separator.Length;
            if ((separator == null) || (separatorCount == 0))
            {
                for (int i = 0; (i < sourceLength) && (num < length); i++)
                {
                    if (char.IsWhiteSpace(source[i]))
                    {
                        sepList[num++] = i;
                    }
                }
                return num;
            }

            for (int j = 0; (j < sourceLength) && (num < length); j++)
            {
                for (int x = 0; x < separatorCount; x++)
                {
                    if (source[j] == separator[x])
                    {
                        sepList[num++] = j;
                        break;
                    }
                }
            }

            return num;
        }

        private static void InternalSplitOmitEmptyEntries(
            string source, int[] sepList, int[] lengthList, int numReplaces,
            out string[] arrStrings, out string[] arrWhitespace)
        {
            int maxStrings = (numReplaces < 0x7fffffff) ? (numReplaces + 1) : 0x7fffffff;
            int maxWhitespace = (maxStrings < 0x7fffffff) ? (maxStrings + 1) : 0x7fffffff;
            string[] arrStrings1 = new string[maxStrings];
            string[] arrWhitespace1 = new string[maxWhitespace];
            int startIndex = 0;
            int lastIndex = -1;
            int actualStringCount = 0;
            for (int i = 0; (i < numReplaces) && (startIndex < source.Length); i++)
            {
                if ((sepList[i] - startIndex) > 0)
                {
                    if (startIndex > 0)
                    {
                        if (lastIndex > -1)
                            arrWhitespace1[actualStringCount] = source.Substring(lastIndex, startIndex - lastIndex);
                        else arrWhitespace1[actualStringCount] = source.Substring(0, startIndex);
                    }
                    else arrWhitespace1[actualStringCount] = string.Empty;
                    arrStrings1[actualStringCount++] = source.Substring(startIndex, sepList[i] - startIndex);
                    lastIndex = sepList[i];
                }
                startIndex = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
                if (actualStringCount == (0x7fffffff - 1))
                {
                    while ((i < (numReplaces - 1)) && (startIndex == sepList[++i]))
                    {
                        startIndex += (lengthList == null) ? 1 : lengthList[i];
                    }
                    break;
                }
            }

            if (startIndex < source.Length)
            {
                if (startIndex > 0)
                {
                    if (lastIndex > -1)
                        arrWhitespace1[actualStringCount] = source.Substring(lastIndex, startIndex - lastIndex);
                    else arrWhitespace1[actualStringCount] = source.Substring(0, startIndex);
                }
                else arrWhitespace1[actualStringCount] = string.Empty;
                arrStrings1[actualStringCount++] = source.Substring(startIndex);
                lastIndex = source.Length;
            }

            if (lastIndex < source.Length)
            {
                arrWhitespace1[actualStringCount] = source.Substring(lastIndex);
            }
            else
            {
                arrWhitespace1[actualStringCount] = string.Empty;
            }

            arrStrings = arrStrings1;
            arrWhitespace = arrWhitespace1;

            if (actualStringCount != maxStrings)
            {
                arrStrings = new string[actualStringCount];
                arrWhitespace = new string[actualStringCount + 1];
                for (int j = 0; j < actualStringCount; j++)
                {
                    arrStrings[j] = arrStrings1[j];
                    arrWhitespace[j] = arrWhitespace1[j];
                }
                arrWhitespace[actualStringCount] = arrWhitespace1[actualStringCount];
            }
        }

        public static string[] SplitWithEscape(string source, char separator, char escape)
        {
            if (source.Length == 0)
                return new string[0];

            var lstString = new List<string>();
            int len = source.Length;
            bool inEscape = false;
            var sb = new StringBuilder();
            for (int j = 0; j < len; j++)
            {
                if (source[j] == escape && !inEscape)
                {
                    inEscape = true;
                    continue;
                }
                if (inEscape)
                {
                    sb.Append(source[j]);
                    inEscape = false;
                }
                else
                {
                    if (source[j] == separator)
                    {
                        if (sb.Length > 0)
                        {
                            lstString.Add(sb.ToString());
                            sb = new StringBuilder();
                        }
                    }
                    else
                    {
                        sb.Append(source[j]);
                    }
                }
            }
            if (sb.Length > 0) lstString.Add(sb.ToString());
            return lstString.ToArray();
        }

    }
}
