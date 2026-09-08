
namespace FEAR.Hosted.Domain.Api.Investigation
{
    public class PostArtifactResponse
    {
        public Guid ExecutionId { get; set; }

        public PostArtifactResponse(Guid executionId)
        {
            this.ExecutionId = executionId;
        }
    }
}