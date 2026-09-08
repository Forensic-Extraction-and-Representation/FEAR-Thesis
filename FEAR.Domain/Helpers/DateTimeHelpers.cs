namespace FEAR.Domain.Helpers
{
    /// <summary>
    /// Provides helper methods for converting between Unix time, Windows file time, and <see cref="DateTime"/>.
    /// </summary>
    public class DateTimeHelper
    {
        /// <summary>
        /// The Unix epoch (January 1, 1970, UTC).
        /// </summary>
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The Windows file time epoch (January 1, 1601, UTC).
        /// </summary>
        private static readonly DateTime WindowsEpoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts a Unix time value (seconds since Unix epoch) to a local <see cref="DateTime"/>.
        /// If the value appears to be in milliseconds, it is converted to seconds.
        /// </summary>
        /// <param name="unixTime">The Unix time value (seconds or milliseconds since 1970-01-01T00:00:00Z).</param>
        /// <returns>The corresponding local <see cref="DateTime"/>.</returns>
        public static DateTime FromUnixTime(long unixTime)
        {
            if (unixTime > 1E11)
                unixTime = unixTime / 1000;

            return UnixEpoch.AddSeconds(unixTime).ToLocalTime();
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> to Unix time (seconds since Unix epoch).
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
        /// <returns>The number of seconds since 1970-01-01T00:00:00Z.</returns>
        public static long ToUnixTime(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }

        /// <summary>
        /// Converts a Windows file time (number of 100-nanosecond intervals since 1601-01-01T00:00:00Z)
        /// to a UTC <see cref="DateTime"/>.
        /// </summary>
        /// <param name="fileTime">The Windows file time value.</param>
        /// <returns>The corresponding UTC <see cref="DateTime"/>.</returns>
        public static DateTime FromWindowsFileTime(long fileTime)
        {
            return DateTime.FromFileTimeUtc(fileTime);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> to Windows file time (number of 100-nanosecond intervals since 1601-01-01T00:00:00Z).
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
        /// <returns>The Windows file time value.</returns>
        public static long ToWindowsFileTime(DateTime dateTime)
        {
            return dateTime.ToFileTimeUtc();
        }
    }
}
