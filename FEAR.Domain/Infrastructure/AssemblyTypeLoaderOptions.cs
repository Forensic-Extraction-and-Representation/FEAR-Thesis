using System.Reflection;

namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Provides options for loading and processing types from assemblies.
    /// Used to specify which assemblies to scan and how to handle types with specific attributes.
    /// </summary>
    public class AssemblyTypeLoaderOptions
    {
        /// <summary>
        /// Gets or sets the list of assemblies to scan for types.
        /// </summary>
        public List<Assembly> Assemblies { get; set; } = new List<Assembly>();

        /// <summary>
        /// An attribute handler is used to map a specific Class as a handler for another.
        /// The use-case of this is to allow for things like disk images to be handled by
        /// a specific handler class that can process the image type. Same for network and memory.
        /// </summary>
        public Dictionary<Type, Action<Type>> AttributeHandlers { get; set; } = new Dictionary<Type, Action<Type>>();
    }
}
