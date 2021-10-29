using System;
using System.Text;

namespace Codenet.Encryption
{
    /// <summary>
    /// An implementation of XXTEA encryption
    /// </summary>
    public static class XXTEA
    {
		private static byte[] EMPTY_DATA = new byte[] {};

        /// <summary>
        /// Encrypt a string
        /// </summary>
        /// <param name="input">input string to encrypt</param>
        /// <param name="key">minimum length is 16 bytes (will be automatically padded)</param>
        /// <returns></returns>
        public static byte[] Encrypt(string input, string key)
        {
            if (input.Length == 0) return EMPTY_DATA;
            if (key.Length == 0) return Encoding.UTF8.GetBytes(input);
            
            return Encrypt(Encoding.UTF8.GetBytes(input), Encoding.UTF8.GetBytes(key));
        }

        /// <summary>
        /// Encrypt a buffer
        /// </summary>
        /// <param name="input">will be automatically aligned to UInt32 boundaries</param>
        /// <param name="key">will be automatically aligned to UInt32 boundaries</param>
        public static byte[] Encrypt(byte[] input, byte[] key)
        {
            if (input.Length == 0) return EMPTY_DATA;
            if (key.Length == 0) return input;

            var v = BytesToLongs(input, 1);
            var k = BytesToLongs(key, 4);

            Encrypt(v, k);

            return LongsToBytes(v);
        }

        /// <summary>
        /// Encrypt a buffer of `UInt32` (in place)
        /// </summary>
        /// <param name="input">minimum length is 1 UInt32</param>
        /// <param name="key">minimum length is 4 UInt32s</param>
        public static void Encrypt(UInt32[] input, UInt32[] key)
        {
            var v = input;
            var k = key;
            int n = v.Length;

            UInt32 z = v[n - 1], y, sum = 0, e, DELTA = 0x9e3779b9, mx;
            UInt32 q = (UInt32)(6 + 52 / n);

            while (q-- > 0)
            {
                sum += DELTA;
                e = sum >> 2 & 3;
                for (int p = 0; p < n; p++)
                {
                    y = v[(p + 1) % n];
                    mx = ((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4)) ^ (sum ^ y) + (k[(p & 3 ^ e)] ^ z);
                    z = v[p] += mx;
                }
            }
        }

        /// <summary>
        /// Decrypt a buffer
        /// </summary>
        /// <param name="input">will be automatically aligned to UInt32 boundaries</param>
        /// <param name="key">will be automatically aligned to UInt32 boundaries</param>
        /// <param name="trimZeroes">should trim 0 bytes at the end of the resulting buffer?</param>
        public static byte[] Decrypt(byte[] input, byte[] key, bool trimZeroes = true)
        {
            if (input.Length == 0) return EMPTY_DATA;
            if (key.Length == 0) return input;
            
            var v = BytesToLongs(input, 1);
            var k = BytesToLongs(key, 4);

            Decrypt(v, k);

            var decryptedBytes = LongsToBytes(v);

            var decryptedLen = decryptedBytes.Length;

            if (trimZeroes)
            {
                while (decryptedLen > 0 && decryptedBytes[decryptedLen - 1] == 0)
                    decryptedLen--;
            }

            if (decryptedLen == 0)
                return EMPTY_DATA;

            if (decryptedLen > decryptedBytes.Length)
            {
                var dest = new byte[decryptedLen];
                Buffer.BlockCopy(decryptedBytes, 0, dest, 0, decryptedLen);
                decryptedBytes = dest;
            }

            return decryptedBytes;
        }

        /// <summary>
        /// Decrypt a buffer of `UInt32` (in place)
        /// </summary>
        /// <param name="input">minimum length is 1 UInt32</param>
        /// <param name="key">minimum length is 4 UInt32s</param>
        public static void Decrypt(UInt32[] input, UInt32[] key)
        {
            var v = input;
            var k = key;
            int n = v.Length;

            UInt32 z, y = v[0], e, DELTA = 0x9e3779b9, mx;
            UInt32 q = (UInt32)(6 + 52 / n);
            UInt32 sum = q * DELTA;

            while (sum != 0)
            {
                e = sum >> 2 & 3;
                for (var p = n - 1; p >= 0; p--)
                {
                    z = v[p > 0 ? p - 1 : n - 1];
                    mx = ((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4)) ^ (sum ^ y) + (k[(p & 3 ^ e)] ^ z);
                    y = v[p] -= mx;
                }
                sum -= DELTA;
            }
        }

        private static UInt32[] BytesToLongs(byte[] s, int padZeroMinLength = 0)
        {
            var slen = s.Length;
            var len = (int)Math.Ceiling(((double)slen) / 4.0d);
            var l = new UInt32[Math.Max(len, padZeroMinLength)];
            
            for (int i = 0, i4 = 0; i < len; i++, i4 += 4)
            {
                l[i] = ((s[i4])) +
                    ((i4 + 1) >= slen ? (UInt32)0 << 8 : ((UInt32)s[i4 + 1] << 8)) +
                    ((i4 + 2) >= slen ? (UInt32)0 << 16 : ((UInt32)s[i4 + 2] << 16)) +
                    ((i4 + 3) >= slen ? (UInt32)0 << 24 : ((UInt32)s[i4 + 3] << 24));
            }
            
            return l;
        }

        private static byte[] LongsToBytes(UInt32[] l)
        {
            var llen = l.Length;
            var b = new byte[llen * 4];
            
            for (int i = 0, i4 = 0; i < llen; i++, i4 += 4)
            {
                var li = l[i];
                b[i4] = (byte)(li & 0xFF);
                b[i4 + 1] = (byte)(li >> (8 & 0xFF));
                b[i4 + 2] = (byte)(li >> (16 & 0xFF));
                b[i4 + 3] = (byte)(li >> (24 & 0xFF));
            }
            
            return b;
        }
    }
}