namespace FEAR.Domain.Telemetry
{
    public interface IFEARTelementryHandlerPredicates
    {
        IList<Predicate<IFEARTelemetrySignal>> SignalPredicates { get; }
    }

    public class GenericFEARTelementryHandlerPredicates : IFEARTelementryHandlerPredicates
    {
        public IList<Predicate<IFEARTelemetrySignal>> SignalPredicates { get; set; } = new List<Predicate<IFEARTelemetrySignal>>();
    }
}
