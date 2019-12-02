using System;
using System.Collections.Generic;
using System.Text;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    class ErrorResponse
    {
        public Error Error { get; set; }

        public string CorrelationId { get; set; }
    }

    class Error
    {
        public string Code { get; set; }

        public string Message { get; set; }
    }
}
