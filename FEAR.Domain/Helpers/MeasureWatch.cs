namespace FEAR.Domain.Helpers
{
    /// <summary>
    /// Provides a utility for measuring the execution time of an action in milliseconds.
    /// Useful for performance profiling and benchmarking code blocks.
    /// </summary>
    public class MeasureWatch
    {
        /// <summary>
        /// Measures the time, in milliseconds, taken to execute the specified <paramref name="action"/>.
        /// </summary>
        /// <param name="action">The action to execute and measure.</param>
        /// <returns>The elapsed time in milliseconds.</returns>
        public static long Measure(Action action)
        {
            // Start the watch before executing the action
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            action();

            // Stop the watch after the action has completed
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }
    }
}
