namespace FEAR.Domain.Collector
{
    /// <summary>
    /// Defines different types of collectors that can be used in the FEAR framework.
    /// </summary>
    [Flags]
    public enum CollectorTypeEnum
    {
        None = 0,
        // Collector operates on a stream object
        Stream = 1,
        // Collector operates on a file
        File = 2,
        // Collector operates on a directory
        Directory = 4,
        // Collector operates on another set of result data
        ResultObject = 8
    }

}
