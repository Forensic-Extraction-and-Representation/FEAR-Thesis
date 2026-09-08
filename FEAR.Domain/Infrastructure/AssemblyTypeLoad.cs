namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Provides functionality to scan assemblies for types with specific attributes and invoke registered handlers for those types.
    /// This enables dynamic discovery and registration of handler classes for various evidence types, plugins, or extensibility points.
    /// </summary>
    public class AssemblyTypeLoad
    {
        /// <summary>
        /// Scans the specified assemblies for types decorated with attributes registered in <see cref="AssemblyTypeLoaderOptions.AttributeHandlers"/>.
        /// For each matching attribute, invokes the corresponding handler with the discovered type.
        /// </summary>
        /// <param name="atlo">Options specifying assemblies to scan and attribute handlers to invoke.</param>
        public static void Execute(AssemblyTypeLoaderOptions atlo)
        {
            foreach (var assembly in atlo.Assemblies)
            {
                // Get all the types in the assembly
                foreach (var type in assembly.GetTypes())
                {
                    // Check if the type has any custom attributes
                    foreach (var attribute in type.GetCustomAttributes(false))
                    {
                        // If the attribute type is registered in the handlers, invoke the handler with the type
                        if (atlo.AttributeHandlers.ContainsKey(attribute.GetType()))
                        {
                            atlo.AttributeHandlers[attribute.GetType()](type);
                        }
                    }
                }
            }
        }
    }
}
