using FEAR.Domain.Collector;
using FEAR.Domain.Infrastructure;

namespace FEAR.Runtime.CFEAR.CollectorResults
{
    /// <summary>
    /// Base class for collectors that operate on result objects within the FEAR framework.
    /// Provides the structure for implementing collectors that process <see cref="ArtifactWorkItem"/> instances.
    /// Inheritors must implement logic for handling specific result object types and define how to process them.
    /// </summary>
    public abstract class ResultCollectorBase : CollectorBase, IFEARCollector<ArtifactWorkItem>
    {
        /// <summary>
        /// The collector type for result object collectors.
        /// </summary>
        protected static CollectorTypeEnum _collectorType = CollectorTypeEnum.ResultObject;

        /// <summary>
        /// Gets the collector type, which is <see cref="CollectorTypeEnum.ResultObject"/> for this base class.
        /// </summary>
        public override CollectorTypeEnum CollectorType => _collectorType;

        /// <summary>
        /// Determines if this collector handles a specific result object type.
        /// Must be implemented by derived classes to indicate which result types they support.
        /// </summary>
        /// <param name="resultType">The result type to check.</param>
        /// <returns>True if the collector can handle the specified result type; otherwise, false.</returns>
        public abstract bool DoesHandleResultObjectType(string resultType);

        /// <summary>
        /// Executes the collector using a generic context object, casting it to the appropriate result context.
        /// </summary>
        /// <param name="context">The context object, expected to be of type <see cref="ICollectorContext{CollectorResult}"/>.</param>
        public override void Execute(object context)
        {
            Execute((ICollectorContext<ArtifactWorkItem>)context);
        }

        /// <summary>
        /// Executes the collector logic for a specific result object context.
        /// Must be implemented by derived classes to define result processing behavior.
        /// </summary>
        /// <param name="context">The result collector context.</param>
        public abstract void Execute(ICollectorContext<ArtifactWorkItem> context);

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultCollectorBase"/> class with the specified resolver.
        /// </summary>
        /// <param name="resolver">The FEAR resolver for component management.</param>
        public ResultCollectorBase(IFEARResolver resolver) : base(resolver)
        {
        }
    }
}
