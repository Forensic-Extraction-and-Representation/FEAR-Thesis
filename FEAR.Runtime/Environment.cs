using FEAR.Domain.Infrastructure;
using FEAR.Runtime.Domain;

namespace FEAR.Runtime
{
    /// <summary>
    /// Provides the runtime environment for FEAR execution, encapsulating the compiler and resolver.
    /// 
    /// This environment is used to coordinate the compilation and resolution of FEAR scripts and components.
    /// It is especially relevant in the context of queueing data (either artifacts directly or results from collector (CFEAR) scripts)
    /// to be processed by graph codify scripts. The <see cref="IFEARCompiler"/> and <see cref="IFEARResolver"/> exposed here
    /// are central to managing the registration, discovery, and execution of collectors and codifiers in the FEAR pipeline.
    /// </summary>
    public class Environment
    {
        /// <summary>
        /// Lazily-initialized FEAR compiler instance, responsible for compiling FEAR source code and managing the resolver.
        /// </summary>
        private Lazy<IFEARCompiler> _compiler = new Lazy<IFEARCompiler>(() => throw new System.NotImplementedException());

        /// <summary>
        /// Gets the FEAR resolver, which manages and resolves modules, interpreters, collectors, codifiers, and rulesets.
        /// </summary>
        public IFEARResolver Resolver => Compiler.Resolver;

        /// <summary>
        /// Gets the FEAR compiler, which coordinates compilation and provides access to the resolver.
        /// </summary>
        public IFEARCompiler Compiler => _compiler.Value;

        /// <summary>
        /// Builds a new FEAR runtime environment using the provided compiler instance.
        /// </summary>
        /// <param name="iFEARCompiler">The FEAR compiler to use for this environment.</param>
        /// <returns>A configured <see cref="Environment"/> instance.</returns>
        public static Environment Build(IFEARCompiler iFEARCompiler)
        {
            var env = new Environment();
            env._compiler = new Lazy<IFEARCompiler>(() => iFEARCompiler);

            return env;
        }
    }
}
