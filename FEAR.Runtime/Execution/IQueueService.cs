namespace FEAR.Runtime.Execution
{
    /// <summary>
    /// Defines a contract for a queue service that manages the processing of artifacts.
    /// Provides methods to start and stop the queue processor and exposes the running state.
    /// </summary>
    public interface IQueueService
    {
        /// <summary>
        /// Gets a value indicating whether the queue processor is currently running.
        /// </summary>
        bool QueueRunning { get; }

        /// <summary>
        /// Starts the queue processor, enabling processing of queued artifacts.
        /// </summary>
        void StartQueueProcessor();

        /// <summary>
        /// Stops the queue processor, halting processing of queued artifacts.
        /// </summary>
        void StopQueueProcessor();

        void StartRuleSetScheduler();
        void StopRuleSetScheduler();

        void StartServices();
        void StopServices();
    }
}
