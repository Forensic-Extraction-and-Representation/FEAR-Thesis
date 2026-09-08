using System.Buffers.Binary;
using System.Dynamic;
using System.Text;
using FEAR.Domain.Interpreter;

namespace FEAR.IFEAR
{
    public class InterpreterContext : IInterpreterContext
    {
        private IFEARInterpreter Interpreter { get; set; }

        public Stream DataStream { get; set; } = new MemoryStream();
        public Dictionary<string, object> ReturnValues { get; set; } = new Dictionary<string, object>();

        public void Skip(int count)
        {
            int newPosition = (int)DataStream.Position + count;
            if (newPosition < 0)
                // This is actually a negative number we are adding to the position
                DataStream.Position = DataStream.Length + newPosition;
            else
                DataStream.Position += count;
        }
        public ulong ReadAsUInt(int count, bool littleEndian)
        {
            byte[] readData = new byte[count];
            DataStream.Read(readData, 0, count);

            switch (count)
            {
                case 1:
                    return readData[0];
                case 2:
                    return littleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(readData) : BinaryPrimitives.ReadUInt16BigEndian(readData);
                case 4:
                    return littleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(readData) : BinaryPrimitives.ReadUInt32BigEndian(readData);
                case 8:
                    return littleEndian ? BinaryPrimitives.ReadUInt64LittleEndian(readData) : BinaryPrimitives.ReadUInt64BigEndian(readData);
                default:
                    throw new Exception("Invalid read count");
            }
        }
        public long ReadAsInt(int count, bool littleEndian)
        {
            byte[] readData = new byte[count];
            DataStream.Read(readData, 0, count);

            switch (count)
            {
                case 1:
                    return readData[0];
                case 2:
                    return littleEndian ? BinaryPrimitives.ReadInt16LittleEndian(readData) : BinaryPrimitives.ReadInt16BigEndian(readData);
                case 4:
                    return littleEndian ? BinaryPrimitives.ReadInt32LittleEndian(readData) : BinaryPrimitives.ReadInt32BigEndian(readData);
                case 8:
                    return littleEndian ? BinaryPrimitives.ReadInt64LittleEndian(readData) : BinaryPrimitives.ReadInt64BigEndian(readData);
                default:
                    throw new Exception("Invalid read count");
            }
        }

        public string ReadString(int count, bool wide)
        {
            byte[] readData = new byte[count];
            DataStream.Read(readData, 0, count);

            if (wide)
                return Encoding.Unicode.GetString(readData);
            else
                return Encoding.ASCII.GetString(readData);
        }

        public T GetReturnObjectValue<T>(string name)
        {
            if (ReturnValues.ContainsKey(name))
                return (T)ReturnValues[name];
            else
                return default(T);
        }

        public void SetReturnObjectValue<T>(string name, T value)
        {
            if (value == null)
                return;

            if (ReturnValues.ContainsKey(name))
                ReturnValues[name] = value;
            else
                ReturnValues.Add(name, value);
        }

        public InterpreterContext(IFEARInterpreter interpreter)
        {
            Interpreter = interpreter;
        }

        public object Execute(Stream data)
        {
            DataStream = data;
            Interpreter.Execute(this);

            return ConvertToExpandoObject(ReturnValues);
        }

        private ExpandoObject ConvertToExpandoObject(Dictionary<string, object> dict)
        {
            var eo = new ExpandoObject();
            var eoColl = (ICollection<KeyValuePair<string, object>>)eo;

            foreach (var kvp in dict)
            {
                eoColl.Add(kvp);
            }

            return eo;
        }

        public object Execute(byte[] data) { return Execute(new MemoryStream(data)); }
    }

}
