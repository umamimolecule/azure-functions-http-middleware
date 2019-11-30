using System;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Exception that is thrown within validators.
    /// </summary>
    [Serializable]
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        public BadRequestException(string message)
            : base(message)
        {
        }
    }
}
