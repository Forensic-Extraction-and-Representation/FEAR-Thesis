namespace FEAR.Host.Domain.Api.Investigation
{
    public class GetInvestigationGraphStatistics
    {
        public class Response
        {
            public bool IsAvailable { get; set; }
            public int TriplesCount { get; set; }
            public int SubjectsCount { get; set; }
            public int PredicatesCount { get; set; }
            public int ObjectsCount { get; set; }
            public int LiteralsCount { get; set; }
        }
    }

    public class GetInvestigationGraphData
    {
        public class Response
        {
            public bool IsAvailable { get; set; }
            public string TurtleContent { get; set; }
        }
    }
}
