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
        /// Initializ
        /// </summary>
        /// <param name="message"></param>
        public BadRequestException(string message)
            : base(message)
        {
        }
    }
}
