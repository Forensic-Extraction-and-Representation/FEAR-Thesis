namespace FEAR.Runtime.DataFormats
{
    /// <summary>
    /// Provides a data format context for handling dynamic object data.
    /// Implements <see cref="IDataFormatContext"/> to support wrapping dynamic objects or lists of dynamic objects
    /// as a data context state, but does not support file or string content parsing.
    /// </summary>
    public class DynamicObjectDataFormatContext : IDataFormatContext
    {
        /// <summary>
        /// Gets the MIME type or identifier for dynamic object content.
        /// There is no specific MIME type for dynamic objects, so this is a placeholder.
        /// </summary>
        public string ContentMimeType => "<DynamicObject>";

        /// <summary>
        /// Gets the supported format identifiers for dynamic object content.
        /// </summary>
        public string[] Format => new string[] { "<DynamicObject>" };

        /// <summary>
        /// Gets the file extension filters (none for dynamic objects).
        /// </summary>
        public string[] FileExtensionFilter => new string[] { };

        /// <summary>
        /// Not supported for dynamic objects. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public IDataFormatContextState CreateStateFromContent(string content)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not supported for dynamic objects. Throws <see cref="NotImplementedException"/>.
        /// </summary>
        public IDataFormatContextState CreateStateFromFile(string file)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new state from a single dynamic object.
        /// </summary>
        /// <param name="object">The dynamic object to wrap.</param>
        /// <returns>A new <see cref="IDataFormatContextState"/> containing the object.</returns>
        public IDataFormatContextState CreateStateFromObject(dynamic @object)
        {
            return DynamicObjectDataFormatContextState.FromObject(@object);
        }

        /// <summary>
        /// Creates a new state from a list of dynamic objects.
        /// </summary>
        /// <param name="list">The list of dynamic objects to wrap.</param>
        /// <returns>A new <see cref="IDataFormatContextState"/> containing the objects.</returns>
        public IDataFormatContextState CreateStateFromObjectList(IEnumerable<dynamic> list)
        {
            return DynamicObjectDataFormatContextState.FromObjectList(list);
        }

        /// <summary>
        /// Represents the state for a dynamic object data context, providing access to the wrapped dynamic data.
        /// </summary>
        public class DynamicObjectDataFormatContextState : IDataFormatContextState
        {
            /// <summary>
            /// Gets or sets the enumerable of dynamic data.
            /// </summary>
            public IEnumerable<dynamic> Data { get; set; } = new List<dynamic>();

            /// <summary>
            /// Creates a new state from a single dynamic object.
            /// </summary>
            /// <param name="object">The dynamic object to wrap.</param>
            /// <returns>A new <see cref="DynamicObjectDataFormatContextState"/> containing the object.</returns>
            public static DynamicObjectDataFormatContextState FromObject(dynamic @object)
            {
                return new DynamicObjectDataFormatContextState()
                {
                    Data = new List<dynamic> { @object }
                };
            }

            /// <summary>
            /// Creates a new state from a list of dynamic objects.
            /// </summary>
            /// <param name="list">The list of dynamic objects to wrap.</param>
            /// <returns>A new <see cref="DynamicObjectDataFormatContextState"/> containing the objects.</returns>
            public static DynamicObjectDataFormatContextState FromObjectList(IEnumerable<dynamic> list)
            {
                return new DynamicObjectDataFormatContextState() { Data = list };
            }
        }
    }
}