using System.Runtime.InteropServices;

namespace FEAR.Domain.Helpers
{
    /// <summary>
    /// Provides helper methods for efficient byte array operations.
    /// </summary>
    public static class ByteHelpers
    {
        /// <summary>
        /// Compares two byte arrays for equality using the native <c>memcmp</c> function for high performance.
        /// </summary>
        /// <param name="b1">The first byte array.</param>
        /// <param name="b2">The second byte array.</param>
        /// <param name="count">The number of bytes to compare.</param>
        /// <returns>Zero if the arrays are equal; otherwise, a nonzero value.</returns>
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        /// <summary>
        /// Determines whether two byte arrays are equal by comparing their contents.
        /// Uses <c>memcmp</c> for efficient comparison.
        /// </summary>
        /// <param name="b1">The first byte array.</param>
        /// <param name="b2">The second byte array.</param>
        /// <returns><c>true</c> if the arrays are equal in length and content; otherwise, <c>false</c>.</returns>
        public static bool IsEqual(this byte[] b1, byte[] b2)
        {
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }
    }
}