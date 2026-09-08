using FEAR.Domain.Infrastructure;
using FEAR.Domain.Provenance;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace FEAR.Runtime.Domain
{
    /// <summary>
    /// Manages temporary directories associated with provenance objects.
    /// Responsible for creating, tracking, and cleaning up temporary directories
    /// used during evidence processing, analysis, or transformation workflows.
    /// </summary>
    public class TemporaryDirectoryManager : ITemporaryDirectoryManager
    {
        /// <summary>
        /// Gets or sets the dictionary of temporary directory entries, keyed by provenance URI.
        /// </summary>
        public Dictionary<string, TemporaryDirectoryEntry> TemporaryDirectoryEntries { get; set; }

        /// <summary>
        /// Gets the root path of the temporary directory managed by this instance.
        /// </summary>
        public string TemporaryDirectory { get; }

        /// <summary>
        /// Lock object for thread-safe operations on the temporary directory entries.
        /// </summary>
        private object tdmLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryDirectoryManager"/> class
        /// using configuration values for the temporary directory path and clear flag.
        /// </summary>
        /// <param name="systemConfiguration">The application configuration.</param>
        public TemporaryDirectoryManager(IConfiguration systemConfiguration) :
            this(systemConfiguration.GetValue<string>("SystemProperties:TemporaryDirectory"),
                systemConfiguration.GetValue<bool>("SystemProperties:ClearTemporary"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryDirectoryManager"/> class
        /// with the specified directory and clear flag.
        /// </summary>
        /// <param name="temporaryDirectory">The path to the temporary directory.</param>
        /// <param name="clearTemporary">Whether to clear the directory on startup.</param>
        public TemporaryDirectoryManager(string temporaryDirectory, bool clearTemporary)
        {
            TemporaryDirectory = temporaryDirectory;

            // Optionally clear the temporary directory if requested
            if (clearTemporary)
            {
                if (Directory.Exists(TemporaryDirectory))
                    Directory.Delete(TemporaryDirectory, true);
            }

            InitialiseTemporaryDirectory();
        }

        /// <summary>
        /// Ensures the temporary directory exists and loads or creates the metadata file (tdm.json).
        /// </summary>
        private void InitialiseTemporaryDirectory()
        {
            if (!Directory.Exists(TemporaryDirectory))
            {
                Directory.CreateDirectory(TemporaryDirectory);
            }

            // Load the tdm.json if it exists; otherwise, create a new one
            if (File.Exists(Path.Combine(TemporaryDirectory, "tdm.json")))
            {
                var json = File.ReadAllText(Path.Combine(TemporaryDirectory, "tdm.json"));
                TemporaryDirectoryEntries = JsonConvert.DeserializeObject<Dictionary<string, TemporaryDirectoryEntry>>(json);
                CleanupTemporaryDirectory();
            }
            else
            {
                TemporaryDirectoryEntries = new Dictionary<string, TemporaryDirectoryEntry>();
                File.WriteAllText(Path.Combine(TemporaryDirectory, "tdm.json"), JsonConvert.SerializeObject(TemporaryDirectoryEntries));
            }
        }

        /// <summary>
        /// Updates the metadata file (tdm.json) with the current state of temporary directory entries.
        /// </summary>
        private void UpdateTemporaryDirectoryMeta()
        {
            File.WriteAllText(Path.Combine(TemporaryDirectory, "tdm.json"), JsonConvert.SerializeObject(TemporaryDirectoryEntries));
        }

        /// <summary>
        /// Retrieves or creates a <see cref="TemporaryDirectoryEntry"/> for the specified provenance object.
        /// </summary>
        /// <param name="prov">The provenance object for which to get or create a temporary directory entry.</param>
        /// <returns>The <see cref="TemporaryDirectoryEntry"/> associated with the provenance.</returns>
        public TemporaryDirectoryEntry GetTemporaryDirectoryEntry(IProvenance prov)
        {
            lock (tdmLock)
            {
                TemporaryDirectoryEntry tde = null;
                var uri = prov.GetProvenanceUri();
                if (TemporaryDirectoryEntries.ContainsKey(uri))
                {
                    tde = TemporaryDirectoryEntries[uri];
                    tde.LastUsed = DateTime.Now;

                    // Remove entry if the path no longer exists
                    if (!Directory.Exists(tde.Path) && !File.Exists(tde.Path))
                    {
                        TemporaryDirectoryEntries.Remove(uri);
                        tde = null;
                    }
                }

                // Create a new entry if one does not exist
                if (tde == null)
                {
                    tde = new TemporaryDirectoryEntry()
                    {
                        ProvenanceUri = uri,
                        Created = DateTime.Now,
                        LastUsed = DateTime.Now,
                        Path = Path.Combine(TemporaryDirectory, Guid.NewGuid().ToString())
                    };

                    TemporaryDirectoryEntries.Add(uri, tde);
                }

                UpdateTemporaryDirectoryMeta();
                return TemporaryDirectoryEntries[uri];
            }
        }

        /// <summary>
        /// Cleans up all managed temporary directories, removing any that are no longer needed.
        /// Currently, this method is disabled and does not perform cleanup.
        /// </summary>
        public void CleanupTemporaryDirectory()
        {
            lock (tdmLock)
            {
                // We don't want it clearing any temporary data at present
                return;

                // The following code is disabled but shows how cleanup would be performed:
                /*
                var toRemove = TemporaryDirectoryEntries.Where(x => x.Value.LastUsed < DateTime.UtcNow.AddDays(-5)).ToList();
                foreach (var t in toRemove)
                {
                    if (File.Exists(t.Value.Path))
                        File.Delete(t.Value.Path);
                    else if (Directory.Exists(t.Value.Path))
                        Directory.Delete(t.Value.Path, true);

                    TemporaryDirectoryEntries.Remove(t.Key);
                }

                UpdateTemporaryDirectoryMeta();
                */
            }
        }

        /// <summary>
        /// Removes a specific temporary directory entry and deletes the associated directory or file from the file system.
        /// </summary>
        /// <param name="entry">The temporary directory entry to remove and clean up.</param>
        public void RemoveTemporaryDirectoryEntry(TemporaryDirectoryEntry entry)
        {
            lock (tdmLock)
            {
                if (File.Exists(entry.Path))
                    File.Delete(entry.Path);
                else if (Directory.Exists(entry.Path))
                    Directory.Delete(entry.Path, true);

                TemporaryDirectoryEntries.Remove(entry.ProvenanceUri);
                UpdateTemporaryDirectoryMeta();
            }
        }
    }
}
