namespace FEAR.Domain.Arguments
{
    /// <summary>
    /// Represents a set of paths to repositories for FEAR scripts.
    /// </summary>
    public class FearRepositoryPaths
    {
        public Parameter<List<string>> IFEAR { get; set; }
        public Parameter<List<string>> CFEAR { get; set; }
        public Parameter<List<string>> GFEAR { get; set; }
        public Parameter<List<string>> RFEAR { get; set; }

        public FearRepositoryPaths()
        {
            IFEAR = new Parameter<List<string>>();
            CFEAR = new Parameter<List<string>>();
            GFEAR = new Parameter<List<string>>();
            RFEAR = new Parameter<List<string>>();
        }
    }
}
