
namespace FEAR.WASM.Interactions
{
    public abstract class BaseUIEvent : IGenericUIEvent
    {
        public string Action { get; set; }
        public DateTime LastChanged => DateTime.Now;
        public BaseUIEvent(string action)
        {
            Action = action;
        }

        public bool IsAction(string[] options)
        {
            if (options == null || options.Length == 0)
                return false;
            foreach (var option in options)
            {
                if (option == Action)
                    return true;
            }
            return false;
        }
    }
}
