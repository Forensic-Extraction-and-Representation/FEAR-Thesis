namespace FEAR.Domain.Api
{
    public abstract class BaseResponse
    {
        /// <summary>
        /// Indicates whether the request was successful.
        /// </summary>
        public bool Success { get; set; } = true;
        /// <summary>
        /// An optional message providing additional information about the response.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
