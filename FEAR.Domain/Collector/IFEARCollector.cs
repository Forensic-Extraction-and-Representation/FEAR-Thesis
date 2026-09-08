namespace FEAR.Domain.Collector
{
    /// <summary>
    /// Interface for a FEAR collector.
    /// </summary>
    public interface IFEARCollector
    {
        /// <summary>
        /// The name of the collector.
        /// </summary>
        string CollectorName { get; }
        /// <summary>
        /// The category of the collector.
        /// This is used to group collectors into categories for easier management and organization.
        /// </summary>
        string CollectorCategory { get; }
        /// <summary>
        /// The type of input the collector processes.
        /// </summary>
        CollectorTypeEnum CollectorType { get; }

        /// <summary>
        /// Executes the collector with the provided data.
        /// </summary>
        /// <param name="data"></param>
        void Execute(object data);
    }

    public interface IFEARCollector<T> : IFEARCollector
    {
        void Execute(ICollectorContext<T> data);
    }

}
