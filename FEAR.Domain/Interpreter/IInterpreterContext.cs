using System.Buffers.Binary;
using System.Text;

namespace FEAR.Domain.Interpreter
{
    /// <summary>
    /// Defines a context for interpreter execution, providing access to the data stream, return values,
    /// and utility methods for reading and manipulating data during interpretation.
    /// This interface is intended for use by FEAR interpreters to abstract the details of data access and result management.
    /// </summary>
    public interface IInterpreterContext
    {
        /// <summary>
        /// Gets the data stream to be interpreted.
        /// </summary>
        Stream DataStream { get; }

        /// <summary>
        /// Gets the dictionary of return values produced by the interpreter.
        /// </summary>
        Dictionary<string, object> ReturnValues { get; }

        /// <summary>
        /// Executes the interpreter logic on the provided data stream.
        /// </summary>
        /// <param name="data">The input data stream.</param>
        /// <returns>The result of the execution.</returns>
        object Execute(Stream data);

        /// <summary>
        /// Executes the interpreter logic on the provided byte array.
        /// </summary>
        /// <param name="data">The input data as a byte array.</param>
        /// <returns>The result of the execution.</returns>
        object Execute(Byte[] data);

        /// <summary>
        /// Skips the specified number of bytes in the data stream.
        /// </summary>
        /// <param name="count">The number of bytes to skip.</param>
        void Skip(int count);

        /// <summary>
        /// Reads an integer value from the data stream.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <param name="littleEndian">Whether to interpret the bytes as little-endian.</param>
        /// <returns>The integer value read from the stream.</returns>
        long ReadAsInt(int count, bool littleEndian);

        /// <summary>
        /// Reads an unsigned integer value from the data stream.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <param name="littleEndian">Whether to interpret the bytes as little-endian.</param>
        /// <returns>The unsigned integer value read from the stream.</returns>
        ulong ReadAsUInt(int count, bool littleEndian);

        /// <summary>
        /// Reads a string from the data stream.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        /// <param name="wide">Whether to read as a wide (Unicode) string.</param>
        /// <returns>The string read from the stream.</returns>
        string ReadString(int count, bool wide);

        /// <summary>
        /// Gets a return value of the specified type from the return values dictionary.
        /// </summary>
        /// <typeparam name="T">The expected type of the return value.</typeparam>
        /// <param name="name">The key of the return value.</param>
        /// <returns>The return value cast to the specified type.</returns>
        T GetReturnObjectValue<T>(string name);

        /// <summary>
        /// Sets a return value in the return values dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="name">The key for the return value.</param>
        /// <param name="value">The value to set.</param>
        void SetReturnObjectValue<T>(string name, T value);
    }
}
