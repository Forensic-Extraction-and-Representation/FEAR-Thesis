using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace FEAR
{
    public enum FEARErrorType
    {
        Semantic,
        MissingProperty,
        TypeMismatch
    }

    public abstract class FEARErrorBase
    {
        /// <summary>
        /// The exact symbol that caused the error.
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// The line number of the error in the original input.
        /// </summary>
        public int Line { get; set; }
        /// <summary>
        /// The character position in the line number of the error in the original input.
        /// </summary>
        public int CharPositionInLine { get; set; }
        /// <summary>
        /// The error message detailing the error.
        /// </summary>
        public string Message { get; set; }
        public FEARErrorType ErrorType { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return $"Error at line {Line}:{CharPositionInLine} - {Message}";
        }

        public FEARErrorBase(int line, int column, string message, string symbol, FEARErrorType errorType)
        {
            Line = line;
            CharPositionInLine = column;
            Message = message;
            Symbol = symbol;
            ErrorType = errorType;
        }
    }

    public class FEARParseError : FEARErrorBase
    {
        public FEARParseError(int line, int column, string message, string symbol, FEARErrorType errorType) : base(line, column, message, symbol, errorType)
        {
        }

        /// <summary>
        /// Whether the error was a 'mismatched input', as opposed to a 'no viable alternative' error.
        /// </summary>
        public bool IsMismatchedInput { get; set; }
    }

}
