
namespace FEAR.WASM.Interactions
{
    public interface IGenericUIEvent
    {
        string Action { get; set; }
        DateTime LastChanged { get; }
        bool IsAction(string[] options);
    }
}
