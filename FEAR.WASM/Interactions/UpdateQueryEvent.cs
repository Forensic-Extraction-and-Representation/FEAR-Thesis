namespace FEAR.WASM.Interactions
{
    public class UpdateQueryEvent
    {
        public UpdateQueryEvent(string queryText)
        {
            QueryText = queryText;
        }
        public string QueryText { get; set; } = string.Empty;
    }
}
