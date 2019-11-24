using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public class HttpFunctionContext : IHttpFunctionContext
    {
        public HttpRequest Request { get; set; }

        public IActionResult Response { get; set; }

        public ILogger Logger { get; set; }

        public string CorrelationId { get; set; }

        public object QueryModel { get; set; }

        public object BodyModel { get; set; }
    }
}
