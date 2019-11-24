using System;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    [Serializable]
    public class BadRequestException : Exception
    {
        public BadRequestException(string message)
            : base(message)
        {
        }
    }
}
