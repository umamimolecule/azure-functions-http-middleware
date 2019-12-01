namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Contains error codes.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// The error code for invalid query parameters.
        /// </summary>
        public const string InvalidQueryParameters = "INVALID_QUERY_PARAMETERS";

        /// <summary>
        /// The error code for invalid body payload.
        /// </summary>
        public const string InvalidBody = "INVALID_BODY";

        /// <summary>
        /// The error code for internal server error.
        /// </summary>
        public const string InternalServerError = "INTERNAL_SERVER_ERROR";

        /// <summary>
        /// The error code for unauthorized.
        /// </summary>
        public const string Unauthorized = "UNAUTHORIZED";
    }
}
