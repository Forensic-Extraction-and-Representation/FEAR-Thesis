using FEAR.Domain.Interpreter;
using FEAR.GFEAR;

namespace FEAR.IFEAR
{
    public abstract class InterpreterBase : IFEARInterpreter
    {
        public abstract void Execute(IInterpreterContext context);
        public abstract string InterpreterName { get; }
        public abstract string InterpreterCategory { get; }

        protected DateTimeOffset FromUnixTimeSeconds(long ticks)
        {
            return DateTimeOffset.FromUnixTimeSeconds(ticks);
        }
        protected DateTime? FromFileTime(long ticks)
        {
            if (ticks > DateTime.UtcNow.AddYears(1000).Ticks)
                return null;
            else
                return DateTime.FromFileTime(ticks);

        }
    }

}
