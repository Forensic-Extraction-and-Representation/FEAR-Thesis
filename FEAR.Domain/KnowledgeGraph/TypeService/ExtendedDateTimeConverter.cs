using FEAR.Domain.Helpers;
using System.ComponentModel;
using System.Globalization;

namespace FEAR.Domain.KnowledgeGraph.TypeService
{
    /// <summary>
    /// Provides extended conversion logic for date and time values.
    /// Supports conversion from Unix time (as string) and standard date/time formats,
    /// and outputs values as <see cref="XmlSchema.xsdDateTime"/> or other representations as needed.
    /// </summary>
    public class ExtendedDateTimeConverter : DateTimeConverter
    {
        /// <summary>
        /// Converts the given value to a <see cref="XmlSchema.xsdDateTime"/> object.
        /// Supports conversion from Unix time (as string) or standard date/time formats.
        /// </summary>
        /// <param name="context">The format context.</param>
        /// <param name="culture">The culture info.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// An <see cref="XmlSchema.xsdDateTime"/> if conversion is successful; otherwise, null.
        /// </returns>
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            DateTime? dt = null;
            // Attempt to parse as Unix time (long in string form)
            if (value is string && long.TryParse((string)value, out long longTime))
            {
                dt = DateTimeHelper.FromUnixTime(longTime);
            }

            // Fallback to base conversion if not Unix time
            if(dt == null)
                dt = (DateTime?)base.ConvertFrom(context, CultureInfo.CurrentCulture, value);

            // Wrap in xsdDateTime if conversion succeeded
            return dt != null ? new XmlSchema.xsdDateTime(dt.Value) : null;
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> value to the specified destination type.
        /// Supports conversion to string (ISO format), long (Unix time), or string (Unix time as string).
        /// </summary>
        /// <param name="context">The format context.</param>
        /// <param name="culture">The culture info.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert to.</param>
        /// <returns>The converted value, or the base implementation if not handled.</returns>
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object value, Type destinationType)
        {
            if (value is DateTime dateTime)
            {
                if (destinationType == typeof(string))
                {
                    // Return ISO 8601 string representation
                    return dateTime.ToUniversalTime().ToString("u");
                }
                else if (destinationType == typeof(long))
                {
                    // Return Unix time as long
                    return DateTimeHelper.ToUnixTime(dateTime);
                }
                else if (destinationType == typeof(string))
                {
                    // Return Unix time as string
                    return DateTimeHelper.ToUnixTime(dateTime).ToString();
                }
            }

            // Fallback to base conversion
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
