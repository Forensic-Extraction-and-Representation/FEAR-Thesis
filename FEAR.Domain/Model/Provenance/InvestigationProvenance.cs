using FEAR.Domain.Provenance;
using System.Text.Json.Serialization;

namespace FEAR.Domain.Model.Provenance
{
    /// <summary>
    /// Represents the root provenance object for an investigation.
    /// Stores the base URI and base folder path for the investigation, and provides logic for generating
    /// file paths based on the provenance hierarchy.
    /// </summary>
    public class InvestigationProvenance : BaseProvenance
    {
        /// <summary>
        /// Gets the base URI for the investigation. This uniquely identifies the investigation in a URI form.
        /// </summary>
        [JsonInclude]
        public Uri BaseUri { get; protected set; }

        /// <summary>
        /// Gets the base folder path for the investigation. This is the root directory for storing investigation data.
        /// </summary>
        [JsonInclude]
        public string BaseFolderPath { get; protected set; }
        
        /// <summary>
        /// Gets the URI element for this provenance object, which is the string representation of the base URI.
        /// </summary>
        public override string ProvenanceUriElement => BaseUri.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationProvenance"/> class.
        /// </summary>
        public InvestigationProvenance() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvestigationProvenance"/> class with the specified name, base URI, and base folder path.
        /// </summary>
        /// <param name="name">The name of the investigation.</param>
        /// <param name="uriBase">The base URI for the investigation.</param>
        /// <param name="baseFolder">The base folder path for the investigation.</param>
        public InvestigationProvenance(string name, Uri uriBase, string baseFolder) : base(null, name)
        {
            BaseUri = uriBase;
            BaseFolderPath = baseFolder;
        }

        /// <summary>
        /// Generates a file path for the specified name by traversing the provenance history and combining
        /// sanitized URI elements. The resulting path is rooted at <see cref="BaseFolderPath"/>.
        /// </summary>
        /// <param name="name">The name of the file to create.</param>
        /// <param name="history">The provenance history to use for path construction.</param>
        /// <returns>The generated file path as a string.</returns>
        public override string CreateFileFor(string name, ref IList<IProvenance> history)
        {
            var path = "";

            foreach (var provenance in history.Reverse())
            {
                string part = System.Text.RegularExpressions.Regex.Replace(provenance.ProvenanceUriElement, "[^a-zA-Z0-9_-]+", "");
                path = System.IO.Path.Combine(path, part);
            }

            return System.IO.Path.Combine(BaseFolderPath, path.ToLower());
        }
    }
}
