namespace FEAR.Runtime.Domain
{
    /// <summary>
    /// Represents options for a FEAR source language, including its directory, file extensions, and language name.
    /// </summary>
    public class FEARSourceOption
    {
        /// <summary>
        /// Gets or sets the directory where the source files for this FEAR language are located.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Gets or sets the list of file extensions associated with this FEAR language.
        /// </summary>
        public List<string> FileExtensions { get; set; }

        /// <summary>
        /// Gets or sets the name of the FEAR language (e.g., "IFEAR", "CFEAR").
        /// </summary>
        public string FEARLanguage { get; set; }
    }
}
