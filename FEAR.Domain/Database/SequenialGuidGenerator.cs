using MassTransit;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace FEAR.Domain.Database
{
    /// <summary>
    /// Generates sequential GUID values for use as primary keys in Entity Framework entities.
    /// This improves index performance compared to random GUIDs, especially for large tables.
    /// </summary>
    public class SequenialGuidGenerator : ValueGenerator<Guid>
    {
        /// <summary>
        /// Indicates that the values generated are permanent and not temporary.
        /// </summary>
        public override bool GeneratesTemporaryValues => false;

        /// <summary>
        /// Generates the next sequential GUID value for the given entity entry.
        /// </summary>
        /// <param name="entry">The entity entry for which the value is being generated.</param>
        /// <returns>A new sequential <see cref="Guid"/>.</returns>
        public override Guid Next(EntityEntry entry)
        {
            return NewId.NextSequentialGuid();
        }

        /// <summary>
        /// Generates the next sequential GUID value for the given entity entry (non-generic override).
        /// </summary>
        /// <param name="entry">The entity entry for which the value is being generated.</param>
        /// <returns>A new sequential <see cref="Guid"/> as an object.</returns>
        protected override object NextValue(EntityEntry entry)
        {
            return NewId.NextSequentialGuid();
        }
    }
}
