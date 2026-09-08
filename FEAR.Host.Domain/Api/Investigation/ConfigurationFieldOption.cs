using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.Host.Domain.Api.Investigation
{
    public class ConfigurationField
    {
        public string? Name { get; set; }
        public List<ConfigurationFieldOption> FieldOptions { get; set; }
    }

    public class ConfigurationFieldOption
    {
        public String? Name { get; set; }
        public String? Value { get; set; } = string.Empty;

        public FieldMetadata[] FieldMetadata { get; set; }
    }

    public class FieldMetadata
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public MetadataValue[]? Values { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSecret { get; set; }
    }

    public class MetadataValue
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public string? Description { get; set; }
    }
}
