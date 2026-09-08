namespace FEAR.Runtime.Helpers
{
    /// <summary>
    /// Provides helper methods for converting Unix epoch-based day counts to <see cref="DateTime"/> values.
    /// </summary>
    public static class UnixHelpers
    {
        /// <summary>
        /// Converts a string representing the number of days since the Unix epoch (1970-01-01) to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="days">A string containing the number of days since 1970-01-01.</param>
        /// <returns>A <see cref="DateTime"/> corresponding to the given number of days since the Unix epoch.</returns>
        internal static DateTime UnixDays(string days)
        {
            int daysInt = int.Parse(days);
            return UnixDays(daysInt);
        }

        /// <summary>
        /// Converts an integer representing the number of days since the Unix epoch (1970-01-01) to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="days">The number of days since 1970-01-01.</param>
        /// <returns>A <see cref="DateTime"/> corresponding to the given number of days since the Unix epoch.</returns>
        internal static DateTime UnixDays(int days)
        {
            return new DateTime(1970, 1, 1).AddDays(days);
        }
    }
}
