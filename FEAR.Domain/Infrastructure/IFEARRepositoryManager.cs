namespace FEAR.Domain.Infrastructure
{
    /// <summary>
    /// Defines a contract for managing and synchronizing FEAR repositories
    /// and precompiled libraries. Implementations of this interface are 
    /// responsible for ensuring that the required repository resources
    /// (such as modules, rulesets, or libraries) are up-to-date and 
    /// available for the application.
    /// </summary>
    public interface IFEARRepositoryManager
    {
        /// <summary>
        /// Synchronizes the specified repository type, ensuring it is 
        /// up-to-date and available. The <paramref name="type"/> parameter 
        /// typically refers to a logical grouping such as "modules", 
        /// "rulesets", or other repository resources.
        /// </summary>
        /// <param name="type">The type or category of repository to synchronize.</param>
        void SynchronizeRepository(string type);

        /// <summary>
        /// Synchronizes precompiled libraries for the specified type, 
        /// ensuring that any required binaries or compiled resources 
        /// are present and current for the application to use.
        /// </summary>
        /// <param name="type">The type or category of precompiled libraries to synchronize.</param>
        void SynchronizePrecompiledLibraries(string type);
        void SynchronizePackageLibraries();
    }
}
