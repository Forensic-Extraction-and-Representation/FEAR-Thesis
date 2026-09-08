namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Represents a standard response wrapper for knowledge graph operations.
    /// Encapsulates a value, error state, message, and exception details.
    /// </summary>
    /// <typeparam name="TValue">The type of the value returned by the operation.</typeparam>
    public class KGResponse<TValue>
    {
        private TValue _value;

        /// <summary>
        /// Gets the value of the response. Throws a <see cref="KGException"/> if an error occurred and <see cref="ThrowOnError"/> is true.
        /// </summary>
        public TValue Value
        {
            get
            {
                if (Error && ThrowOnError)
                    throw new KGException(Message, Exception);

                return _value;
            }
            protected set => _value = value;
        }

        /// <summary>
        /// Gets a value indicating whether an error occurred.
        /// </summary>
        public bool Error { get; protected set; }

        /// <summary>
        /// Gets the error message, if any.
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// Gets the exception associated with the error, if any.
        /// </summary>
        public Exception Exception { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether to throw an exception when accessing <see cref="Value"/> if an error occurred.
        /// </summary>
        public bool ThrowOnError { get; protected set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="KGResponse{TValue}"/> class.
        /// </summary>
        public KGResponse()
        {
        }

        /// <summary>
        /// Marks the response as successful and sets the value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void AsSuccess(TValue value)
        {
            Value = value;
            Error = false;
        }

        /// <summary>
        /// Marks the response as an error with an optional message and exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="excewption">The exception associated with the error.</param>
        public void AsError(string message = null, Exception excewption = null)
        {
            Error = true;
            Message = message;
            Exception = excewption;
        }

        /// <summary>
        /// Marks the response as an error using the provided exception.
        /// </summary>
        /// <param name="exception">The exception to associate with the error.</param>
        public void AsError(Exception exception)
        {
            AsError();
            Message = exception.Message;
            Exception = exception;
        }

        /// <summary>
        /// Marks the response as successful and sets the value, returning the response for chaining.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>The current response instance.</returns>
        public KGResponse<TValue> WithSuccess(TValue value)
        {
            AsSuccess(value);
            return this;
        }

        /// <summary>
        /// Marks the response as an error using the provided exception, returning the response for chaining.
        /// </summary>
        /// <param name="exception">The exception to associate with the error.</param>
        /// <returns>The current response instance.</returns>
        public KGResponse<TValue> WithError(Exception exception)
        {
            AsError(exception);
            return this;
        }

        /// <summary>
        /// Marks the response as an error with a message and optional exception, returning the response for chaining.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="exception">The exception associated with the error.</param>
        /// <returns>The current response instance.</returns>
        public KGResponse<TValue> WithError(string message, Exception exception = null)
        {
            AsError(message, exception);
            return this;
        }

        /// <summary>
        /// Returns an error response if the subject is null, otherwise returns a success response.
        /// </summary>
        /// <param name="subject">The value to check for null.</param>
        /// <param name="errorMessage">The error message if the subject is null.</param>
        /// <returns>The current response instance.</returns>
        public KGResponse<TValue> WithErrorIfNull(TValue? subject, string errorMessage)
        {
            if (subject == null)
                return WithError(errorMessage);
            else
                return WithSuccess(subject);
        }

        /// <summary>
        /// Gets a value indicating whether the response is successful.
        /// </summary>
        public bool IsSuccess => !Error;

        /// <summary>
        /// Gets a value indicating whether the response is an error.
        /// </summary>
        public bool IsError => Error;
    }

    /// <summary>
    /// Represents a standard response wrapper for knowledge graph operations with an additional response enum.
    /// </summary>
    /// <typeparam name="TValue">The type of the value returned by the operation.</typeparam>
    /// <typeparam name="TResponseEnum">The enum type representing response status or error codes.</typeparam>
    public class KGResponse<TValue, TResponseEnum> : KGResponse<TValue>
        where TResponseEnum : Enum
    {
        /// <summary>
        /// Gets the response enum value representing the status or error code.
        /// </summary>
        public TResponseEnum ResponseEnum { get; private set; }

        /// <summary>
        /// Marks the response as an error with the specified enum value.
        /// </summary>
        /// <param name="error">The enum value representing the error.</param>
        /// <returns>The current response instance.</returns>
        public KGResponse<TValue, TResponseEnum> WithError(TResponseEnum error)
        {
            ResponseEnum = error;
            base.AsError();
            return this;
        }

        /// <summary>
        /// Marks the response as an error with the specified enum value and message.
        /// </summary>
        /// <param name="error">The enum value representing the error.</param>
        /// <param name="message">The error message.</param>
        /// <returns>The current response instance.</returns>
        public KGResponse<TValue, TResponseEnum> WithError(TResponseEnum error, string message)
        {
            ResponseEnum = error;
            base.AsError(message);
            return this;
        }

        /// <summary>
        /// Marks the response as an error with the specified enum value and exception.
        /// </summary>
        /// <param name="error">The enum value representing the error.</param>
        /// <param name="exception">The exception associated with the error.</param>
        /// <returns>The current response instance.</returns>
        public KGResponse<TValue, TResponseEnum> WithError(TResponseEnum error, Exception exception)
        {
            ResponseEnum = error;
            base.AsError(exception);
            return this;
        }

        /// <summary>
        /// Marks the response as an error with the specified enum value, message, and exception.
        /// </summary>
        /// <param name="error">The enum value representing the error.</param>
        /// <param name="message">The error message.</param>
        /// <param name="exception">The exception associated with the error.</param>
        /// <returns>The current response instance.</returns>
        public KGResponse<TValue, TResponseEnum> WithError(TResponseEnum error, string message, Exception exception)
        {
            ResponseEnum = error;
            base.AsError(message, exception);
            return this;
        }

        /// <summary>
        /// Marks the response as successful with the specified enum value and value.
        /// </summary>
        /// <param name="response">The enum value representing the response status.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The current response instance.</returns>
        public KGResponse<TValue, TResponseEnum> WithSuccess(TResponseEnum response, TValue value)
        {
            ResponseEnum = response;
            Value = value;
            return this;
        }
    }
}
