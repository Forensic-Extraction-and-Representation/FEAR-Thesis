namespace FEAR.Domain.Helpers
{
    /// <summary>
    /// Provides helper methods for mapping .NET types to their corresponding XML Schema (XSD) type URIs.
    /// </summary>
    public class TypeUriHelpers
    {
        /// <summary>
        /// Returns the XSD type URI string for a given .NET <see cref="Type"/>.
        /// </summary>
        /// <param name="t">The .NET type to map.</param>
        /// <returns>The XSD type URI string corresponding to the .NET type.</returns>
        public static string GetTypeUriOfType(Type t)
        {
            if (t == typeof(int))
                return "xsd:XMLSchema#integer";
            else if (t == typeof(DateTime) || t==typeof(DateTime?))
                return "xsd:XMLSchema#dateTime";
            else if (t == typeof(long))
                return "xsd:XMLSchema#long";
            else if (t == typeof(double))
                return "xsd:XMLSchema#double";
            else if (t == typeof(decimal))
                return "xsd:XMLSchema#decimal";
            else if (t == typeof(float))
                return "xsd:XMLSchema#float";
            else if (t == typeof(bool))
                return "xsd:XMLSchema#boolean";
            else
                return "xsd:XMLSchema#string";
        }

        /// <summary>
        /// Gets the XSD type URI string for the .NET <see cref="bool"/> type.
        /// </summary>
        public static string Boolean => GetTypeUriOfType(typeof(bool));

        /// <summary>
        /// Gets the XSD type URI string for the .NET <see cref="string"/> type.
        /// </summary>
        public static string String => GetTypeUriOfType(typeof(string));

        /// <summary>
        /// Gets the XSD type URI string for the .NET <see cref="int"/> type.
        /// </summary>
        public static string Integer => GetTypeUriOfType(typeof(int));

        /// <summary>
        /// Gets the XSD type URI string for the .NET <see cref="long"/> type.
        /// </summary>
        public static string Long => GetTypeUriOfType(typeof(long));

        /// <summary>
        /// Gets the XSD type URI string for the .NET <see cref="double"/> type.
        /// </summary>
        public static string Double => GetTypeUriOfType(typeof(double));

        /// <summary>
        /// Gets the XSD type URI string for the .NET <see cref="float"/> type.
        /// </summary>
        public static string Float => GetTypeUriOfType(typeof(float));

        /// <summary>
        /// Gets the XSD type URI string for the .NET <see cref="decimal"/> type.
        /// </summary>
        public static string Decimal => GetTypeUriOfType(typeof(decimal));

        /// <summary>
        /// Gets the XSD type URI string for the .NET <see cref="DateTime"/> type.
        /// </summary>
        public static string DateTime => GetTypeUriOfType(typeof(DateTime));
    }
}
