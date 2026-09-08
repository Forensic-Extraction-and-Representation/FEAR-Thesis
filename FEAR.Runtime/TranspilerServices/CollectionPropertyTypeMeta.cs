namespace FEAR.Runtime.TranspilerServices
{
    /// <summary>
    /// Represents metadata for a property that is a collection of another type.
    /// </summary>
    public class CollectionPropertyTypeMeta : PropertyTypeMeta
    {
        /// <summary>
        /// The type of items collected in the collection property.
        /// </summary>
        public string CollectedType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionPropertyTypeMeta"/> class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="type">The property type.</param>
        /// <param name="collectedType">The type of items in the collection.</param>
        public CollectionPropertyTypeMeta(string name, string type, string collectedType) : base(name, type)
        {
            CollectedType = collectedType;
            EphemeralName = EphemeralName + "Collection";
        }
    }
}
