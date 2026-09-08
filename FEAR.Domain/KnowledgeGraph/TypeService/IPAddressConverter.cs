using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace FEAR.Domain.KnowledgeGraph.TypeService
{
    /// <summary>
    /// Custom JSON converter for serializing and deserializing <see cref="IPAddress"/> and <see cref="List{IPAddress}"/> objects.
    /// Handles conversion between IP address objects and their string representations in JSON.
    /// </summary>
    public class IPAddressConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this converter can handle the specified object type.
        /// Supports <see cref="IPAddress"/> and <see cref="List{IPAddress}"/>.
        /// </summary>
        /// <param name="objectType">The type of the object to check.</param>
        /// <returns>True if the type is supported; otherwise, false.</returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(IPAddress)) return true;
            if (objectType == typeof(List<IPAddress>)) return true;

            return false;
        }

        /// <summary>
        /// Reads JSON and converts it into an <see cref="IPAddress"/> or <see cref="List{IPAddress}"/> object.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The type of object to create.</param>
        /// <param name="existingValue">The existing value of the object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The deserialized <see cref="IPAddress"/> or <see cref="List{IPAddress}"/>.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Convert a string to an IPAddress object
            if (objectType == typeof(IPAddress))
            {
                var t = JToken.Load(reader).ToString();
                if (string.IsNullOrEmpty(t))
                    return null;

                return IPAddress.Parse(t);
            }

            // Convert an array of strings to a List<IPAddress>
            if (objectType == typeof(List<IPAddress>))
            {
                return JToken.Load(reader).Select(address => IPAddress.Parse((string)address)).ToList();
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes an <see cref="IPAddress"/> or <see cref="List{IPAddress}"/> object as JSON.
        /// </summary>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Serialize an IPAddress object as a string
            if (value.GetType() == typeof(IPAddress))
            {
                JToken.FromObject(value.ToString()).WriteTo(writer);
                return;
            }

            // Serialize a List<IPAddress> as an array of strings
            if (value.GetType() == typeof(List<IPAddress>))
            {
                JToken.FromObject((from n in (List<IPAddress>)value select n.ToString()).ToList()).WriteTo(writer);
                return;
            }

            throw new NotImplementedException();
        }
    }
}
