using System;

namespace FEAR.Host.Domain.Database.Model
{
    /// <summary>
    /// Represents a system-wide property with a name and value.
    /// </summary>
    public class SystemWideProperty
    {
        /// <summary>
        /// Gets or sets the unique identifier for the system-wide property.
        /// </summary>
        public Guid SystemWidePropertyId { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public string Value { get; set; }
    }
}