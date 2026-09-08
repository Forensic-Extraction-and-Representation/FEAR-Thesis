namespace FEAR.Runtime.Helpers
{
    /// <summary>
    /// Provides extension methods for string manipulation.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a copy of the input string with the first character converted to uppercase.
        /// Throws an <see cref="ArgumentNullException"/> if the input is null, or an <see cref="ArgumentException"/> if the input is empty.
        /// </summary>
        /// <param name="input">The string to capitalize.</param>
        /// <returns>The input string with its first character in uppercase.</returns>
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
    }
}
