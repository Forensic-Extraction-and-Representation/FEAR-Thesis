using FEAR.Domain.Infrastructure;

namespace FEAR.Runtime.Domain
{
    /// <summary>
    /// Defines a contract for a FEAR compiler, which is responsible for compiling FEAR source code
    /// using registered resolvers and collector searchers. The compiler coordinates the resolution
    /// of modules, interpreters, collectors, and codifiers, and manages the search and registration
    /// of collectors for different source types.
    /// </summary>
    public interface IFEARCompiler
    {
        /// <summary>
        /// Gets the FEAR resolver, which manages and resolves modules, interpreters, collectors, codifiers, and rulesets.
        /// </summary>
        IFEARResolver Resolver { get; }

        /// <summary>
        /// Gets the container for managing and searching collector searchers.
        /// </summary>
        IFEARCollectorSearcherContainer CollectorSearcherContainer { get; }

        /// <summary>
        /// Compiles FEAR source code of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of FEAR source to compile.</typeparam>
        /// <param name="name">The name or path of the source to compile.</param>
        /// <param name="outFile">The output file path for the compiled result.</param>
        void CompileFEARSource<T>(string name, string outFile);
    }
}
