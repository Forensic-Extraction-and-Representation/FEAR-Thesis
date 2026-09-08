namespace FEAR.Runtime.TranspilerServices
{
    /// <summary>
    /// Specifies the type of relationship a property has to its parent entity.
    /// </summary>
    public enum RelationType
    {
        /// <summary>
        /// The property is a simple data type (e.g., int, string).
        /// </summary>
        DataType,
        /// <summary>
        /// The property is a reference to another object/entity.
        /// </summary>
        Object,
        /// <summary>
        /// The property is a collection of objects/entities.
        /// </summary>
        Collection
    }
}
