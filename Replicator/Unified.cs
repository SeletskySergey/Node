using System;

/// <summary>
/// Unified unique codes generator designed by Sergey Seletsky
/// </summary>
public static class Unified
{
    // select shift for (x16 >> 4 == 1) dimensions
    private const int x16Shift = 4;

    // select shift for (x32 >> 5 == 1) dimensions
    private const int x32Shift = 5;

    // select shift for (x64 >> 6 == 1) dimensions
    private const int x64Shift = 6;

    /// <summary>
    /// FNV x64 Prime https://en.wikipedia.org/wiki/Prime_number
    /// </summary>
    private const ulong prime = 1099511628211U;

    /// <summary>
    /// FNV x64 Offset basis
    /// </summary>
    private const ulong offset = 14695981039346656037U;

    // Set of symbols used for numeric dimensions transcoding
    private static char[] symbols = new char[64]
    {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F',
            'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V',
            'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
            'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '$', '@'
    };

    // Generate x64 FNV hash based on random GUID
    public static ulong NewHash()
    {
        return NewHash(Guid.NewGuid().ToByteArray());
    }

    // Generate x64 FNV hash based on data bytes
    public static ulong NewHash(byte[] bytes)
    {
        var hash = offset; // fnv offset basis

        foreach (var @byte in bytes)
        {
            hash ^= @byte;
            hash *= prime; // fnv prime
        }

        return hash;
    }

    // Generate random x32 hex
    public static string NewHex()
    {
        return NewHex32(NewHash());
    }

    // Generate x32 hex from number
    public static string NewHex16(ulong hash)
    {
        return NewHex(hash, x16Shift, 16);
    }

    // Generate x32 hex from number
    public static string NewHex32(ulong hash)
    {
        return NewHex(hash, x32Shift, 13);
    }

    // Generate x64 hex from number
    public static string NewHex64(ulong hash)
    {
        return NewHex(hash, x64Shift, 11);
    }

    private static string NewHex(ulong hash, int shift, int length)
    {
        char[] hex = new char[length--];
        for (var grade = length; grade >= 0; grade--)
        {
            // index = >> hash >> slice(shift * grade) & dimension(1<<shift) - 1
            var index = (byte)((uint)(hash >> shift * grade) & (1 << shift) - 1);
            hex[length - grade] = symbols[index];
        }
        return new string(hex);
    }

    // Decode x16 hex to number
    public static ulong Decode16(string hex)
    {
        return Decode(hex, x16Shift);
    }

    // Decode x32 hex to number
    public static ulong Decode32(string hex)
    {
        return Decode(hex, x32Shift);
    }

    // Decode x64 hex to number
    public static ulong Decode64(string hex)
    {
        return Decode(hex, x64Shift);
    }

    /// <summary>
    /// Decode HEX to Number
    /// </summary>
    /// <param name="hex">String HEX</param>
    /// <param name="shift">Shift of (dimension >> 1 ... == 1)</param>
    /// <returns>Unsigned x64 integer</returns>
    private static ulong Decode(string hex, int shift)
    {
        ulong hash = 0;
        for (int i = 0; i < hex.Length; i++)
        {
            var index = (ulong)Array.IndexOf(symbols, hex[i]);
            // slice grade and convert to number
            var grade = index << ((hex.Length - 1 - i) * shift);
            hash += grade;
        }
        return hash;
    }
}
