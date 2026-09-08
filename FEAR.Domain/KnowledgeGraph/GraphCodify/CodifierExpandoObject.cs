using System.Dynamic;

namespace FEAR.Domain.KnowledgeGraph.GraphCodify
{
    /// <summary>
    /// Provides a dynamic wrapper around <see cref="NullingExpandoObject"/> for flexible property access.
    /// Supports implicit conversion from <see cref="ExpandoObject"/> and creation from plain objects.
    /// </summary>
    public class CodifierExpandoObject
    {
        // Internal dynamic storage for property values.
        NullingExpandoObject internalValue = new NullingExpandoObject();

        /// <summary>
        /// Implicitly converts an <see cref="ExpandoObject"/> to a <see cref="CodifierExpandoObject"/>.
        /// </summary>
        /// <param name="value">The <see cref="ExpandoObject"/> to convert.</param>
        public static implicit operator CodifierExpandoObject(ExpandoObject value)
        {
            return new CodifierExpandoObject() { internalValue = new NullingExpandoObject(value) };
        }

        /// <summary>
        /// Creates a <see cref="CodifierExpandoObject"/> from a plain object by copying its public properties.
        /// </summary>
        /// <param name="obj">The source object.</param>
        /// <returns>A new <see cref="CodifierExpandoObject"/> with properties from the source object.</returns>
        public static CodifierExpandoObject FromObject(object obj)
        {
            IDictionary<string, object> expando = new NullingExpandoObject();

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var currentValue = propertyInfo.GetValue(obj);
                try
                {
                    expando.Add(propertyInfo.Name, currentValue);
                }
                catch
                {
                    // Ignore duplicate or invalid property additions.
                }
            }
            return new CodifierExpandoObject() { internalValue = expando as NullingExpandoObject };
        }

        /// <summary>
        /// Gets or sets a dynamic property by name.
        /// Returns null if the property does not exist.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The property value, or null if not found.</returns>
        public dynamic this[string propertyName]
        {
            get
            {
                if (((IDictionary<string, object>)internalValue).ContainsKey(propertyName))
                    return ((IDictionary<string, object>)internalValue)[propertyName];
                else
                    return null;
            }
            set
            {
                ((IDictionary<string, object>)internalValue)[propertyName] = value;
            }
        }

        /// <summary>
        /// Gets the internal dictionary of property values.
        /// </summary>
        public IDictionary<string, object> InternalValue => internalValue;
    }
}
