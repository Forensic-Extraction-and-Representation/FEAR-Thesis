using FEAR.Domain.Collector;
using FEAR.Domain.Provenance;
using FEAR.Runtime.Execution;

namespace FEAR.Runtime
{
    public class ArtifactWorkResult
    {

    }

    /// <summary>
    /// Represents the result of a collector operation within the FEAR framework.
    /// Encapsulates the collected entity, its provenance, and optional metadata 
    /// about the collector and properties.
    /// </summary>
    public class ArtifactWorkItem
    {
        public List<KeyValuePair<string, bool>> PipelineStages { get; set; } = new List<KeyValuePair<string, bool>>();

        public string CurrentStage => PipelineStages.FirstOrDefault(t=>t.Value == false).Key;
        public string NextStage => PipelineStages.SkipWhile(ps => ps.Key != CurrentStage).Skip(1).FirstOrDefault().Key;

        public void MarkStageComplete(string stageName)
        {
            var stage = PipelineStages.FirstOrDefault(ps => ps.Key == stageName);
            if (!stage.Equals(default(KeyValuePair<string, bool>)))
            {
                PipelineStages.Remove(stage);
                PipelineStages.Add(new KeyValuePair<string, bool>(stage.Key, true));
            }
        }
        public bool IsStageComplete(string stageName)
        {
            var stage = PipelineStages.FirstOrDefault(ps => ps.Key == stageName);
            return !stage.Equals(default(KeyValuePair<string, bool>)) && stage.Value;
        }
        public bool IsComplete => PipelineStages.All(ps => ps.Value);

        /// <summary>
        /// The provenance information describing the origin, history, and relationships of the collected entity.
        /// </summary>
        public IProvenance Source { get; set; }

        /// <summary>
        /// The entity result produced by the collector, containing type and entity information.
        /// </summary>
        public IEntityResult Entity { get; set; }

        /// <summary>
        /// The name of the item that produced this result, typically the collector name.
        /// </summary>
        public string ArtifactWorkItemGenerator { get; }
        public IArtifactPipelineExecution PipelineArtifactExecution { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactWorkItem"/> class with provenance and entity information.
        /// </summary>
        /// <param name="source">The provenance source of the collected entity.</param>
        /// <param name="entity">The collected entity result.</param>
        public ArtifactWorkItem(IProvenance source , IEntityResult entity) { 
            Source = source;
            Entity = entity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactWorkItem"/> class with provenance, entity, and collector name.
        /// </summary>
        /// <param name="source">The provenance source of the collected entity.</param>
        /// <param name="entity">The collected entity result.</param>
        /// <param name="collectorName">The name of the collector that produced this result.</param>
        public ArtifactWorkItem(IProvenance source, IEntityResult entity, string collectorName) : this(source, entity)
        {
            ArtifactWorkItemGenerator = collectorName;
        }
    }
}
