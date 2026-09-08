namespace FEAR.Domain.Module
{
    /// <summary>
    /// Attribute used to mark a class as a FEAR module.
    /// This attribute ties a specific module implementation to a unique name, enabling dynamic discovery,
    /// registration, and resolution of modules within the FEAR framework.
    /// The <see cref="FullName"/> property provides a unique identifier for the module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FEARModuleAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Gets the full name of the module, which is the same as <see cref="ModuleName"/>.
        /// </summary>
        public string FullName => ModuleName;
    }
}
