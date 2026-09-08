
namespace FEAR.WASM.Interactions
{
    public class SendSessionEvent : BaseUIEvent
    {
        public string RequestType { get; internal set; }
        public string SubmitData { get; set; }

        public SendSessionEvent(string requestType, string submitData) : base(GenericUIEventConstants.SESSION_SEND)
        {
            SubmitData = submitData;
            RequestType = requestType;
        }
    }
}
