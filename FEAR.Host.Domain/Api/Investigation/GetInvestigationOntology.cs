namespace FEAR.Host.Domain.Api.Investigation
{
    public class GetInvestigationOntology
    {
        public class Response
        {
            public string OntologyContent { get; set; }
            public bool IsAvailable { get; set; }
        }
    }
}
