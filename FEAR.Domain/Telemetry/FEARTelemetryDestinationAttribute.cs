namespace FEAR.Domain.Telemetry
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class FEARTelemetryDestinationAttribute : Attribute
    {
        public string DestinationName { get; set; }
        public FEARTelemetryDestinationAttribute(string destinationName)
        {
            DestinationName = destinationName;
        }
    }
}
