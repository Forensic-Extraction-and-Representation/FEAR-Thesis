namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Represents errors that occur within the knowledge graph domain.
    /// Use this exception to signal domain-specific issues encountered during graph operations.
    /// </summary>
    public class KGException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KGException"/> class with a specified error message
        /// and an optional inner exception that caused this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or null if none.</param>
        public KGException(string message, Exception innerException = null) : base(message, innerException)
        {

        }
    }
}
