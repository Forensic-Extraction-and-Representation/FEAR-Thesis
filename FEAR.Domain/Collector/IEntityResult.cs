namespace FEAR.Domain.Collector
{
    /// <summary>
    /// Interface for entity results in the collector context.
    /// </summary>
    public interface IEntityResult
    {
        string InternalType { get; }
        string EntityTypeName { get; }
    }
}
