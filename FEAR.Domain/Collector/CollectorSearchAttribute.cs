namespace FEAR.Domain.Collector
{
    /// <summary>
    /// This attribute applied to a collector search class that is part of a collector.
    /// The collector search class is used to define the search parameters for the collector, 
    /// such as file and folder paths that apply to the collector.
    /// </summary>
    public class CollectorSearchAttribute : Attribute
    {
        /// <summary>
        /// The ype of the collector that this attribute is associated with.
        /// </summary>
        public Type For { get; set; }
    }
}
