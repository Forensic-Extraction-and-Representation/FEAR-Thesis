using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

namespace FEAR.Domain.KnowledgeGraph.GraphCodify
{
    /// <summary>
    /// A dynamic object and dictionary implementation that returns null for missing keys or properties.
    /// Useful for scenarios where missing values should not throw exceptions, such as dynamic property access in codification.
    /// </summary>
    public class NullingExpandoObject : DynamicObject, IDynamicMetaObjectProvider, IDictionary<string, object>
    {
        // Internal dictionary to store property values.
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new, empty instance of <see cref="NullingExpandoObject"/>.
        /// </summary>
        public NullingExpandoObject() { }

        /// <summary>
        /// Initializes a new instance with a single value, stored under the key "_VALUE".
        /// </summary>
        /// <param name="value">The value to store.</param>
        public NullingExpandoObject(object value)
        {
            values.Add("_VALUE", value);
        }

        /// <summary>
        /// Initializes a new instance from an existing dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to copy values from.</param>
        public NullingExpandoObject(IDictionary<string, object> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                values.Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Gets or sets a value by key. Returns null if the key does not exist.
        /// </summary>
        public object this[string key]
        {
            get
            {
                if (values.ContainsKey(key))
                    return values[key];
                else
                    return null;
            }
            set
            {
                values[key] = value;
            }
        }

        /// <summary>
        /// Gets the collection of keys.
        /// </summary>
        public ICollection<string> Keys => values.Keys;

        /// <summary>
        /// Gets the collection of values.
        /// </summary>
        public ICollection<object> Values => values.Values;

        /// <summary>
        /// Gets the number of key-value pairs.
        /// </summary>
        public int Count => values.Count;

        /// <summary>
        /// Always returns false; this dictionary is not read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds a key-value pair to the dictionary.
        /// </summary>
        public void Add(string key, object value) => values.Add(key, value);

        /// <summary>
        /// Adds a key-value pair to the dictionary.
        /// </summary>
        public void Add(KeyValuePair<string, object> item) => values.Add(item.Key, item.Value);

        /// <summary>
        /// Removes all key-value pairs from the dictionary.
        /// </summary>
        public void Clear() => values.Clear();

        /// <summary>
        /// Determines whether the dictionary contains a specific key-value pair.
        /// </summary>
        public bool Contains(KeyValuePair<string, object> item) => values.Contains(item);

        /// <summary>
        /// Determines whether the dictionary contains a specific key.
        /// </summary>
        public bool ContainsKey(string key) => values.ContainsKey(key);

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => values.GetEnumerator();

        /// <summary>
        /// Removes the value with the specified key.
        /// </summary>
        public bool Remove(string key) => values.Remove(key);

        /// <summary>
        /// Removes the specified key-value pair.
        /// </summary>
        public bool Remove(KeyValuePair<string, object> item) => values.Remove(item.Key);

        /// <summary>
        /// Tries to get a member value dynamically. Returns null if the property does not exist.
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // Always return true, but result is null if not found.
            values.TryGetValue(binder.Name, out result);
            return true;
        }

        /// <summary>
        /// Not implemented. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a member value dynamically.
        /// </summary>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            values[binder.Name] = value;
            return true;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
    }
}
