using System;
using System.Text;

    /// <summary>
    /// Unified unique codes generator designed by Sergey Seletsky
    /// </summary>
    public static class Unified
    {
        // FNV x64 Prime
        private const ulong prime = 14695981039346656037U;

        // FNV x64 Offset
        private const ulong offset = 1099511628211U;

        // Set of symbols used for numeric dimensions transcoding
        private static char[] symbols = new char[64]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '?', '@'
        };

        // Generate x64 FNV hash based on random GUID
        public static ulong NewHash()
        {
            return NewHash(Guid.NewGuid().ToByteArray());
        }

        // Generate x64 FNV hash based on data bytes
        public static ulong NewHash(byte[] bytes)
        {
            var hash = prime; // fnv prime

            foreach (var @byte in bytes)
            {
                hash ^= @byte;
                hash *= offset; // fnv offset
            }

            return hash;
        }

        // Generate random x32 hex
        public static String NewHex()
        {
            return NewHex(NewHash());
        }

        // Generate x32 hex from number
        public static String NewHex16(ulong n)
        {
            var len = 16;
            char[] ch = new char[len--];
            for (var i = len; i >= 0; i--)
            {
                var inx = (byte)((uint)(n >> 4 * i) & 15);
                ch[len - i] = symbols[inx];
            }
            return new String(ch);
        }

        // Generate x32 hex from number
        public static String NewHex(ulong n)
        {
            var len = 13;
            char[] ch = new char[len--];
            for (var i = len; i >= 0; i--)
            {
                var inx = (byte)((uint)(n >> 5 * i) & 31);
                ch[len - i] = symbols[inx];
            }
            return new String(ch);
        }

        // Generate x64 hex from number
        public static String NewHex64(ulong n)
        {
            var len = 11;
            char[] ch = new char[len--];
            for (var i = len; i >= 0; i--)
            {
                var inx = (byte)((uint)(n >> 6 * i) & 63);
                ch[len - i] = symbols[inx];
            }
            return new String(ch);
        }

        // Decode x16 hex to number
        public static ulong Decode16(string code)
        {
            var shift = 4; // select shift for x64 dimensions if lower case detected
            ulong hash = 0;
            for (int i = 0; i < code.Length; i++)
            {
                var index = (ulong)Array.IndexOf(symbols, code[i]);
                var nuim = (index << ((code.Length - 1 - i) * shift)); // convert dimension to number and add 
                hash += nuim;
            }

            return hash;
        }

        // Decode x32 hex to number
        public static ulong Decode(string code)
        {
            var shift = 5; // select shift for x64 dimensions if lower case detected
            ulong hash = 0;
            for (int i = 0; i < code.Length; i++)
            {
                var index = (ulong)Array.IndexOf(symbols, code[i]);
                var nuim = (index << ((code.Length - 1 - i) * shift)); // convert dimension to number and add 
                hash += nuim;
            }

            return hash;
        }

        // Decode x64 hex to number
        public static ulong Decode64(string code)
        {
            var shift = 6; // select shift for x64 dimensions if lower case detected
            ulong hash = 0;
            for (int i = 0; i < code.Length; i++)
            {
                var index = (ulong)Array.IndexOf(symbols, code[i]);
                var nuim = (index << ((code.Length - 1 - i) * shift)); // convert dimension to number and add 
                hash += nuim;
            }

            return hash;
        }
    }
