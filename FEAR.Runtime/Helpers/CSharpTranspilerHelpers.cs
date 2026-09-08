namespace FEAR.Runtime.Helpers
{
    /// <summary>
    /// Provides helper methods for C# code generation during transpilation.
    /// </summary>
    public static class CSharpTranspilerHelpers
    {
        /// <summary>
        /// Returns a string consisting of the specified number of indentation levels (4 spaces per level).
        /// Useful for formatting generated C# code with proper indentation.
        /// </summary>
        /// <param name="count">The number of indentation levels.</param>
        /// <returns>A string with (count * 4) spaces.</returns>
        public static string Tab(int count) => new string(' ', count*4);
    }
}
