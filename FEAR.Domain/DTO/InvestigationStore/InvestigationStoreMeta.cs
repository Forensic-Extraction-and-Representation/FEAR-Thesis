using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEAR.Domain.Dto.InvestigationStore
{
    /// <summary>
    /// Represents metadata for an investigation's storage configuration.
    /// Used to track and manage details about how and where investigation data is stored.
    /// </summary>
    [Table("InvestigationStoreMeta")]
    public class InvestigationStoreMeta
    {
        /// <summary>
        /// Gets or sets the unique identifier for the investigation associated with this metadata.
        /// </summary>
        [Key]
        public Guid InvestigationId { get; set; }

        /// <summary>
        /// Gets or sets the storage details as a comma-separated string.
        /// This string encodes the list of configured storage backends or options for the investigation.
        /// </summary>
        public string StoreDetails { get; set; } = "";

        /// <summary>
        /// Gets or sets the list of configured storage backends or options for the investigation.
        /// This property is not mapped to the database and is derived from <see cref="StoreDetails"/>.
        /// </summary>
        [NotMapped]
        public List<string> ConfiguredStores
        {
            get => !string.IsNullOrEmpty(StoreDetails) && StoreDetails.Contains(",") ? StoreDetails.Split(',').ToList() : new List<string>();
            set => StoreDetails = string.Join(',', value);
        }
    }
}
