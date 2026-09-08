using System;

namespace FEAR.Domain.Helpers
{
    /// <summary>
    /// Provides helper methods for converting between hexadecimal strings and byte arrays.
    /// Useful for encoding and decoding binary data as hex, such as for cryptographic or serialization purposes.
    /// </summary>
    public class HexHelpers
    {
        /// <summary>
        /// Converts a hexadecimal string (optionally prefixed with "0x") to a byte array.
        /// Handles odd-length strings by prepending a zero.
        /// </summary>
        /// <param name="value">The hexadecimal string to convert (e.g., "0x00ff" or "00ff").</param>
        /// <returns>The corresponding byte array.</returns>
        public static byte[] StringToBytes(string value)
        {
            if (value.StartsWith("0x"))
                value = value.Substring(2);

            if (value.Length % 2 != 0)
                value = "0" + value;

            var bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = byte.Parse(value.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);

            return bytes;
        }

        /// <summary>
        /// Converts a byte array to a lowercase hexadecimal string with no prefix.
        /// </summary>
        /// <param name="value">The byte array to convert.</param>
        /// <returns>The hexadecimal string representation (e.g., "00ff").</returns>
        public static string BytesToString(byte[] value)
        {
            return BitConverter.ToString(value).Replace("-", "").ToLowerInvariant();
        }
    }
}