namespace FEAR.Domain.Collector
{
    /// <summary>
    /// Used to mark a class as a FEAR collector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FEARCollectorAttribute : Attribute
    {
        /// <summary>
        /// The name of the collector.
        /// </summary>
        public string CollectorName { get; set; }
        public string FullName => CollectorName;

        public FEARCollectorAttribute(string name)
        {
            CollectorName = name;
        }
    }
}
