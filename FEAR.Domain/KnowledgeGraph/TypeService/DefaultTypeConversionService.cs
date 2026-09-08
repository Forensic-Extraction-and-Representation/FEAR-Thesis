using System.ComponentModel;

namespace FEAR.Domain.KnowledgeGraph.TypeService
{
    /// <summary>
    /// Provides default type conversion services for RDF and XSD datatypes.
    /// Maps XSD type URIs to .NET <see cref="TypeConverter"/>s and performs value conversions.
    /// </summary>
    public class DefaultTypeConversionService : ITypeConversionService
    {
        /// <summary>
        /// Dictionary mapping XSD type URIs to their corresponding .NET <see cref="TypeConverter"/> implementations.
        /// </summary>
        private static Dictionary<string, TypeConverter> converters = new Dictionary<string, TypeConverter>(StringComparer.OrdinalIgnoreCase)
        {
            {ConversionTypes.ANYURI, new UriTypeConverter()},
            {ConversionTypes.BASE64BINARY, new ByteConverter()},
            {ConversionTypes.BOOLEAN, new BooleanConverter()},
            {ConversionTypes.BYTE, new ByteConverter()},
            {ConversionTypes.DATE, new DateTimeConverter()},
            {ConversionTypes.DATETIME, new ExtendedDateTimeConverter()},
            {ConversionTypes.DATETIMESTAMP, new ExtendedDateTimeConverter()},
            {ConversionTypes.DAYTIMEDURATION, new TimeSpanConverter()},
            {ConversionTypes.DECIMAL, new DecimalConverter()},
            {ConversionTypes.DOUBLE, new DoubleConverter()},
            {ConversionTypes.FLOAT, new SingleConverter()},
            {ConversionTypes.GDAY, new DateTimeConverter()},
            {ConversionTypes.GMONTH, new DateTimeConverter()},
            {ConversionTypes.GMONTHDAY, new DateTimeConverter()},
            {ConversionTypes.GYEAR, new DateTimeConverter()},
            {ConversionTypes.GYEARMONTH, new DateTimeConverter()},
            {ConversionTypes.HEXBINARY, new StringConverter()},
            {ConversionTypes.INT, new Int32Converter()},
            {ConversionTypes.INTEGER, new Int32Converter()},
            {ConversionTypes.LANGUAGE, new StringConverter()},
            {ConversionTypes.LONG, new Int64Converter()},
            {ConversionTypes.NAME, new StringConverter()},
            {ConversionTypes.NCNAME, new StringConverter()},
            {ConversionTypes.NMTOKEN, new StringConverter()},
            {ConversionTypes.NEGATIVEINTEGER, new Int32Converter()},
            {ConversionTypes.NONNEGATIVEINTEGER, new Int32Converter()},
            {ConversionTypes.NONPOSITIVEINTEGER, new Int32Converter()},
            {ConversionTypes.NORMALIZEDSTRING, new StringConverter()},
            {ConversionTypes.POSITIVEINTEGER, new Int32Converter()},
            {ConversionTypes.SHORT, new Int16Converter()},
            {ConversionTypes.STRING, new StringConverter()},
            {ConversionTypes.TIME, new DateTimeConverter()},
            {ConversionTypes.TOKEN, new StringConverter()},
            {ConversionTypes.UNSIGNEDBYTE, new ByteConverter()},
            {ConversionTypes.UNSIGNEDINT, new UInt32Converter()},
            {ConversionTypes.UNSIGNEDLONG, new UInt64Converter()},
            {ConversionTypes.UNSIGNEDSHORT, new UInt16Converter()},
            {ConversionTypes.YEARMONTHDURATION, new TimeSpanConverter()},
            {ConversionTypes.PRECISIONDECIMAL, new DecimalConverter()},
            {ConversionTypes.DURATION, new TimeSpanConverter()},
            {ConversionTypes.QNAME, new StringConverter()},
            {ConversionTypes.ENTITY, new StringConverter()},
            {ConversionTypes.ID, new StringConverter()},
            {ConversionTypes.IDREF, new StringConverter()},
            {ConversionTypes.NOTATION, new StringConverter()},
            {ConversionTypes.IDREFS, new StringConverter()},
            {ConversionTypes.ENTITIES, new StringConverter()},
            {ConversionTypes.NMTOKENS, new StringConverter()}
        };

        private static Dictionary<Type, string> converterMap = new Dictionary<Type, string>()
        {
            {typeof(Uri), ConversionTypes.ANYURI},
            {typeof(byte[]), ConversionTypes.BASE64BINARY},
            {typeof(bool), ConversionTypes.BOOLEAN},
            {typeof(byte), ConversionTypes.BYTE},
            {typeof(DateTime), ConversionTypes.DATETIME},
            {typeof(decimal), ConversionTypes.DECIMAL},
            {typeof(double), ConversionTypes.DOUBLE},
            {typeof(float), ConversionTypes.FLOAT},
            {typeof(int), ConversionTypes.INT},
            {typeof(long), ConversionTypes.LONG},
            {typeof(short), ConversionTypes.SHORT},
            {typeof(string), ConversionTypes.STRING}
        };

        /// <summary>
        /// Gets the <see cref="TypeConverter"/> for the specified XSD type URI.
        /// Returns a <see cref="StringConverter"/> if no specific converter is found.
        /// </summary>
        /// <param name="typeUri">The XSD type URI.</param>
        /// <returns>The corresponding <see cref="TypeConverter"/>.</returns>
        public TypeConverter GetConverterForTypeUri(string typeUri)
        {
            typeUri = typeUri.Replace("http://www.w3.org/2001/XMLSchema#", "xsd:");

            if (converters.ContainsKey(typeUri.ToLower()))
            {
                return converters[typeUri.ToLower()];
            }
            else
            {
                return new StringConverter();
            }
        }

        /// <summary>
        /// Converts the given value to the specified XSD type using the appropriate <see cref="TypeConverter"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="objectTypeUri">The XSD type URI to convert to.</param>
        /// <param name="throwOnError">If true, throws an exception on conversion error; otherwise, returns the original value.</param>
        /// <returns>The converted value, or the original value if conversion fails and <paramref name="throwOnError"/> is false.</returns>
        public object ConvertFromString(string value, string objectTypeUri, bool throwOnError)
        {
            object object_value = value;
            if (objectTypeUri.ToLower() != ConversionTypes.STRING)
            {
                TypeConverter converter = this.GetConverterForTypeUri(objectTypeUri);

                try
                {
                    var entry_object_converted = converter.ConvertFromInvariantString(value.ToString());
                    object_value = entry_object_converted;
                }
                catch (Exception ex)
                {
                    if (throwOnError)
                        throw ex;
                }
            }

            return object_value;
        }

        public string ConvertToString(object value, string objectTypeUri, bool throwOnError)
        {
            TypeConverter converter = this.GetConverterForTypeUri(objectTypeUri);
            try
            {
                return converter.ConvertToInvariantString(value);
            }
            catch (Exception ex)
            {
                if (throwOnError)
                    throw ex;
                return value.ToString();
            }
        }

        string xsdBaseUri = "http://www.w3.org/2001/XMLSchema#";
        public Uri GetTypeUriForObject(object sourceObject)
        {
            if (sourceObject == null)
            {
                return null;
            }
            Type type = sourceObject.GetType();
            if (converterMap.ContainsKey(type))
            {
                return new Uri(converterMap[type].Replace("xsd:", xsdBaseUri));
            }
            else
            {
                return new Uri(ConversionTypes.STRING.Replace("xsd:", xsdBaseUri));
            }
        }

        public Uri GetTypeUriForObjecType<T>()
        {
            Type type = typeof(T);
            if (converterMap.ContainsKey(type))
            {
                return new Uri(converterMap[type].Replace("xsd:", xsdBaseUri));
            }
            else
            {
                return new Uri(ConversionTypes.STRING.Replace("xsd:", xsdBaseUri));
            }
        }
    }
}
