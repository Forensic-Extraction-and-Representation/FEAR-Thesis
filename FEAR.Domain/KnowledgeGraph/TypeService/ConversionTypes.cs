namespace FEAR.Domain.KnowledgeGraph.TypeService
{
    /// <summary>
    /// Provides string constants for XML Schema Definition (XSD) data types.
    /// These types are used for RDF data modeling and conversion, following the W3C XSD Datatypes specification.
    /// </summary>
    /// <remarks>
    /// Reference: https://www.w3.org/2011/rdf-wg/wiki/XSD_Datatypes
    /// </remarks>
    public static class ConversionTypes
    {
        // URI reference to any resource
        public static readonly string ANYURI = new string("xsd:anyURI");
        // Base64-encoded binary data
        public static readonly string BASE64BINARY = new string("xsd:base64Binary");
        // Boolean value (true/false)
        public static readonly string BOOLEAN = new string("xsd:boolean");
        // 8-bit signed integer
        public static readonly string BYTE = new string("xsd:byte");
        // Date (year, month, day)
        public static readonly string DATE = new string("xsd:date");
        // Date and time (with optional timezone)
        public static readonly string DATETIME = new string("xsd:dateTime");
        // Date and time with required timezone
        public static readonly string DATETIMESTAMP = new string("xsd:dateTimeStamp");
        // Duration in days and time
        public static readonly string DAYTIMEDURATION = new string("xsd:dayTimeDuration");
        // Arbitrary-precision decimal number
        public static readonly string DECIMAL = new string("xsd:decimal");
        // 64-bit double-precision floating point
        public static readonly string DOUBLE = new string("xsd:double");
        // 32-bit single-precision floating point
        public static readonly string FLOAT = new string("xsd:float");
        // Day of the month (---DD)
        public static readonly string GDAY = new string("xsd:gDay");
        // Month (--MM--)
        public static readonly string GMONTH = new string("xsd:gMonth");
        // Month and day (--MM-DD)
        public static readonly string GMONTHDAY = new string("xsd:gMonthDay");
        // Year (YYYY)
        public static readonly string GYEAR = new string("xsd:gYear");
        // Year and month (YYYY-MM)
        public static readonly string GYEARMONTH = new string("xsd:gYearMonth");
        // Hex-encoded binary data
        public static readonly string HEXBINARY = new string("xsd:hexBinary");
        // 32-bit signed integer
        public static readonly string INT = new string("xsd:int");
        // Arbitrary-precision integer
        public static readonly string INTEGER = new string("xsd:integer");
        // Language tag (e.g., en, fr)
        public static readonly string LANGUAGE = new string("xsd:language");
        // 64-bit signed integer
        public static readonly string LONG = new string("xsd:long");
        // XML Name
        public static readonly string NAME = new string("xsd:Name");
        // XML NCName (non-colonized name)
        public static readonly string NCNAME = new string("xsd:NCName");
        // XML NMTOKEN (name token)
        public static readonly string NMTOKEN = new string("xsd:NMTOKEN");
        // Negative integer (< 0)
        public static readonly string NEGATIVEINTEGER = new string("xsd:negativeInteger");
        // Non-negative integer (>= 0)
        public static readonly string NONNEGATIVEINTEGER = new string("xsd:nonNegativeInteger");
        // Non-positive integer (<= 0)
        public static readonly string NONPOSITIVEINTEGER = new string("xsd:nonPositiveInteger");
        // Whitespace-normalized string
        public static readonly string NORMALIZEDSTRING = new string("xsd:normalizedString");
        // Positive integer (> 0)
        public static readonly string POSITIVEINTEGER = new string("xsd:positiveInteger");
        // 16-bit signed integer
        public static readonly string SHORT = new string("xsd:short");
        // Character string
        public static readonly string STRING = new string("xsd:string");
        // Time (hh:mm:ss)
        public static readonly string TIME = new string("xsd:time");
        // Whitespace-collapsed string
        public static readonly string TOKEN = new string("xsd:token");
        // 8-bit unsigned integer
        public static readonly string UNSIGNEDBYTE = new string("xsd:unsignedByte");
        // 32-bit unsigned integer
        public static readonly string UNSIGNEDINT = new string("xsd:unsignedInt");
        // 64-bit unsigned integer
        public static readonly string UNSIGNEDLONG = new string("xsd:unsignedLong");
        // 16-bit unsigned integer
        public static readonly string UNSIGNEDSHORT = new string("xsd:unsignedShort");
        // Duration in years and months
        public static readonly string YEARMONTHDURATION = new string("xsd:yearMonthDuration");
        // Decimal with explicit precision
        public static readonly string PRECISIONDECIMAL = new string("xsd:precisionDecimal");
        // General duration (PnYnMnDTnHnMnS)
        public static readonly string DURATION = new string("xsd:duration");
        // Qualified name (namespace:name)
        public static readonly string QNAME = new string("xsd:QName");
        // XML ENTITY type
        public static readonly string ENTITY = new string("xsd:ENTITY");
        // XML ID type
        public static readonly string ID = new string("xsd:ID");
        // XML IDREF type (reference to ID)
        public static readonly string IDREF = new string("xsd:IDREF");
        // XML NOTATION type
        public static readonly string NOTATION = new string("xsd:NOTATION");
        // List of XML IDREFs
        public static readonly string IDREFS = new string("xsd:IDREFS");
        // List of XML ENTITY values
        public static readonly string ENTITIES = new string("xsd:ENTITIES");
        // List of XML NMTOKEN values
        public static readonly string NMTOKENS = new string("xsd:NMTOKENS");
    }
}
