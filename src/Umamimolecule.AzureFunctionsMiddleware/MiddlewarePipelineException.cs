using System;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    [Serializable]
    public class MiddlewarePipelineException : Exception
    {
        public MiddlewarePipelineException()
            : base("Middleware pipeline must be configured with at least one middleware and the final middleware must return a response")
        {
        }
    }
}
