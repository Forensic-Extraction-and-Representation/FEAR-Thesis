
namespace FEAR.WASM.Interactions
{
    public class  StatusEvent : BaseUIEvent
    {
        public string StatusText { get; set; }
        public StatusEvent() : base(GenericUIEventConstants.STATUS_UPDATE)
        {
        }
    }

    public class TimeoutStatusEvent : StatusEvent
    {
        public int Timeout { get; set; } = 2000; // Default timeout in milliseconds
     
        public TimeoutStatusEvent() : base()
        {
            Timeout = 0;
        }
    }

    public class ResolvableStatusEvent : StatusEvent
    {
        public string ResolveIdentifier { get; set; }
        public bool IsResolved { get; set; }
        public ResolvableStatusEvent() : base()
        {
        }
    }
}
