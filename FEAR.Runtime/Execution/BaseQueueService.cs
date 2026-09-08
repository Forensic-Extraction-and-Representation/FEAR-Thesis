
using FEAR.Domain.Infrastructure;
using FEAR.Domain.Telemetry;
using Microsoft.Extensions.Logging;

namespace FEAR.Runtime.Execution
{
    /// <summary>
    /// Provides a base implementation for a queue service that manages the processing of artifacts.
    /// Handles queue processor lifecycle, logging, and thread management.
    /// Derived classes must implement queue-specific logic.
    /// </summary>
    public abstract class BaseQueueService : IQueueService
    {
        /// <summary>
        /// Gets or sets a value indicating whether the queue processor should process items in parallel.
        /// </summary>
        public bool Parallel { get; protected set; } = false;

        /// <summary>
        /// The logger used for diagnostic and error output.
        /// </summary>
        protected IFEARTelemetrySignalService TelemetryService;

        protected IFEARTelemetryDestinationProvider TelemetryProvider { get; set; }
        protected IFEARTelemetryDestination TelemetryDestination { get; set; }
        protected FearExecutionOptions FearExecutionOptions { get; set; }

        /// <summary>
        /// Tracks the last time the queue count was logged.
        /// </summary>
        private DateTime _lastLogTime = DateTime.MinValue;

        /// <summary>
        /// The maximum number of items to dequeue in a single run.
        /// </summary>
        private int MAX_DEDQUEUE_SIZE = 30;

        /// <summary>
        /// The maximum number of rule sets to run in a single execution cycle.
        /// </summary>
        private int MAX_RULESET_RUN = 5;

        /// <summary>
        /// The thread running the queue processor.
        /// </summary>
        protected Thread queueServiceThread { get; set; }

        /// <summary>
        /// The thread running the rule set scheduler.
        /// </summary>
        protected Thread ruleSetSchedulerThread { get; set; }

        /// <summary>
        /// Token source for signaling cancellation to the queue processor.
        /// </summary>
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private Lazy<IServiceProvider> _serviceProvider = null;
        public IServiceProvider ServiceProvider => _serviceProvider.Value;

        /// <summary>
        /// Gets a value indicating whether the queue processor is currently running.
        /// </summary>
        public bool QueueRunning { get; protected set; } = false;

        public bool RuleSetSchedulerRunning { get; protected set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseQueueService"/> class.
        /// </summary>
        /// <param name="logger">The logger to use for output.</param>
        public BaseQueueService(IFEARTelemetrySignalService telemetryService, IFEARTelemetryDestinationProvider telemetryProvider, IServiceProvider serviceProvider, FearExecutionOptions options)
        {
            TelemetryService = telemetryService;
            TelemetryProvider = telemetryProvider;
            FearExecutionOptions = options;
            _serviceProvider = new Lazy<IServiceProvider>(() => serviceProvider);
        }

        /// <summary>
        /// Runs the queue logic for up to <paramref name="maxDequeueSize"/> items.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <param name="maxDequeueSize">The maximum number of items to dequeue in one run.</param>
        /// <returns>The number of items processed.</returns>
        protected abstract int InternalRunQueue(int maxDequeueSize);

        /// <summary>
        /// Runs up to <paramref name="maxDequeueSize"/> rule sets over the graph.
        /// </summary>
        /// <param name="maxDequeueSize"></param>
        /// <returns></returns>
        protected abstract int InternalRunRuleSet(int maxDequeueSize);

        /// <summary>
        /// Logs the current queue count. Must be implemented by derived classes.
        /// </summary>
        protected abstract void LogQueueCount();

        protected IExecutionServiceProvider CreateExecutionServiceProvider()
        {
            return new ExecutionServiceProvider(ServiceProvider);
        }

        /// <summary>
        /// Main loop for the queue processor. Handles logging, cancellation, and error handling.
        /// </summary>
        /// <param name="token">Cancellation token to signal processor shutdown.</param>
        protected void RunQueue(CancellationToken token)
        {
            while (true)
            {
                // Log queue count every 5 seconds
                if (_lastLogTime < DateTime.UtcNow.AddSeconds(-5))
                {
                    LogQueueCount();
                    _lastLogTime = DateTime.UtcNow;
                }

                if (token.IsCancellationRequested)
                {
                    FEARHostedTelemetry.SendExecutionTelemetry("Cancellation Requested", TelemetryService, FEARTelemetrySignalTypeEnum.Environment);
                    break;
                }

                int count = 0;
                try
                {
                    count = InternalRunQueue(MAX_DEDQUEUE_SIZE);
                    PostRunQueueAction(count);
                }
                catch (Exception ex)
                {
                    FEARHostedTelemetry.SendExecutionTelemetry(ex.Message, TelemetryService, FEARTelemetrySignalTypeEnum.Exception);
                }

                // If no items were processed, sleep briefly to avoid busy-waiting
                if (count == 0)
                    Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Main loop for the rule set scheduler. Handles scheduling and execution of rule sets.
        /// </summary>
        /// <param name="token"></param>
        protected virtual void RunRuleSetScheduler(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    FEARHostedTelemetry.SendExecutionTelemetry("Cancellation Requested", TelemetryService, FEARTelemetrySignalTypeEnum.Environment);
                    break;
                }

                int count = 0;
                try
                {
                    count = InternalRunRuleSet(MAX_RULESET_RUN);
                    PostRunRuleSetAction(count);
                }
                catch (Exception ex)
                {
                    FEARHostedTelemetry.SendExecutionTelemetry(ex.Message, TelemetryService, FEARTelemetrySignalTypeEnum.Exception);
                }

                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }

        /// <summary>
        /// Optional action to perform after each queue run. Can be overridden by derived classes.
        /// </summary>
        /// <param name="count">The number of items processed in the last run.</param>
        public virtual void PostRunQueueAction(int count) { }

        public virtual void PostRunRuleSetAction(int count) { }

        /// <summary>
        /// Stops the queue processor and waits for the thread to terminate.
        /// </summary>
        public virtual void StopQueueProcessor()
        {
            FEARHostedTelemetry.SendExecutionTelemetry("Stopping Evidence Queue Processor", TelemetryService, FEARTelemetrySignalTypeEnum.Environment);
            if (QueueRunning)
            {
                QueueRunning = false;
                if (queueServiceThread != null)
                {
                    cancellationTokenSource.Cancel();
                }
            }

            var terminated = queueServiceThread.Join(TimeSpan.FromSeconds(15));
            if (!terminated)
            {
                FEARHostedTelemetry.SendExecutionTelemetry("Evidence Queue Processor did not terminate in 120 seconds", TelemetryService, FEARTelemetrySignalTypeEnum.Environment);
            }
        }

        /// <summary>
        /// Stops the rule set scheduler and waits for the thread to terminate.
        /// </summary>
        public virtual void StopRuleSetScheduler()
        {
            FEARHostedTelemetry.SendExecutionTelemetry("Stopping Rule Set Scheduler", TelemetryService, FEARTelemetrySignalTypeEnum.Environment);
            if (RuleSetSchedulerRunning)
            {
                RuleSetSchedulerRunning = false;
                if (ruleSetSchedulerThread != null)
                {
                    cancellationTokenSource.Cancel();
                }
            }

            var terminated = ruleSetSchedulerThread.Join(120000);
            if (!terminated)
            {
                FEARHostedTelemetry.SendExecutionTelemetry("Rule Set Scheduler did not terminate in 120 seconds", TelemetryService, FEARTelemetrySignalTypeEnum.Environment);
            }
        }

        /// <summary>
        /// Validates any preconditions before starting the queue processor.
        /// Can be overridden by derived classes.
        /// </summary>
        protected virtual void ValidateStartConditions() { }

        /// <summary>
        /// Starts the rule set scheduler if it is not already running.
        /// </summary>
        public virtual void StartRuleSetScheduler()
        {
            if (!RuleSetSchedulerRunning)
            {
                ValidateStartConditions();

                if (ruleSetSchedulerThread != null)
                {
                    cancellationTokenSource.Cancel();
                }

                cancellationTokenSource = new CancellationTokenSource();

                ThreadStart ts = new ThreadStart(() =>
                {
                    RuleSetSchedulerRunning = true;
                    try
                    {
                        RunRuleSetScheduler(cancellationTokenSource.Token);
                    }
                    catch
                    {
                    }

                    RuleSetSchedulerRunning = false;
                });

                ruleSetSchedulerThread = new Thread(ts);
                ruleSetSchedulerThread.Start();
            }
        }

        /// <summary>
        /// Starts the queue processor if it is not already running.
        /// Handles thread and cancellation token management.
        /// </summary>
        public virtual void StartQueueProcessor()
        {
            if (!QueueRunning)
            {
                ValidateStartConditions();

                if (queueServiceThread != null)
                {
                    cancellationTokenSource.Cancel();
                }

                cancellationTokenSource = new CancellationTokenSource();

                ThreadStart ts = new ThreadStart(() =>
                {
                    QueueRunning = true;
                    try
                    {
                        RunQueue(cancellationTokenSource.Token);
                    }
                    catch
                    {
                    }

                    QueueRunning = false;
                });

                queueServiceThread = new Thread(ts);
                queueServiceThread.Start();
            }
        }

        /// <summary>
        /// Starts all services required by this queue service.
        /// </summary>
        public virtual void StartServices()
        {
            TelemetryProvider.RegisterDestination("LogFile", "BaseQueueService", new Dictionary<string, object> { { "LogFileName", $"fear-queueservice-{DateTime.UtcNow:yyyyMMddHHmm}.log" } });
            TelemetryService.SendSignal(new GenericFEARTelemetrySignal("BaseQueueService", FEARTelemetrySerializationEnum.Raw, FEARTelemetrySignalTypeEnum.Environment)
                .WithSignalData("Starting Services"));
            StartQueueProcessor();
            StartRuleSetScheduler();
        }

        /// <summary>
        /// Stops all services managed by this queue service.
        /// </summary>
        public virtual void StopServices()
        {
            StopQueueProcessor();
            StopRuleSetScheduler();
        }
    }
}
