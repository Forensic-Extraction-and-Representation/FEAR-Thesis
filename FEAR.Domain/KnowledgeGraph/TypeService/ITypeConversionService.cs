using System.ComponentModel;

namespace FEAR.Domain.KnowledgeGraph.TypeService
{
    /// <summary>
    /// Defines a service for converting values between .NET types and RDF/XSD datatypes.
    /// Provides methods to retrieve type converters and perform type-safe conversions.
    /// </summary>
    public interface ITypeConversionService
    {
        /// <summary>
        /// Gets a <see cref="TypeConverter"/> for the specified XSD type URI.
        /// </summary>
        /// <param name="typeUri">The XSD type URI (e.g., "xsd:string", "xsd:dateTime").</param>
        /// <returns>A <see cref="TypeConverter"/> capable of converting to and from the specified type.</returns>
        TypeConverter GetConverterForTypeUri(string typeUri);

        /// <summary>
        /// Converts the given value to the specified XSD type using the appropriate converter.
        /// </summary>
        /// <param name="objectValue">The value to convert.</param>
        /// <param name="objectTypeUri">The XSD type URI to convert to.</param>
        /// <param name="throwError">If true, throws an exception on conversion error; otherwise, returns the original value.</param>
        /// <returns>The converted value, or the original value if conversion fails and <paramref name="throwError"/> is false.</returns>
        object ConvertFromString(string objectValue, string objectTypeUri, bool throwError);
        string ConvertToString(object objectValue, string objectTypeUri, bool throwError);
        Uri GetTypeUriForObject(object sourceObject);
        Uri GetTypeUriForObjecType<T>();
    }
}