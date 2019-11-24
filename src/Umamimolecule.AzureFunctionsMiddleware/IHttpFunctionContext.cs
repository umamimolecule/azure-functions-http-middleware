using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public interface IHttpFunctionContext
    {
        HttpRequest Request { get; }
        IActionResult Response { get; set; }
        ILogger Logger { get; }
        string CorrelationId { get; set; }
        object QueryModel { get; set; }
        object BodyModel { get; set; }
    }
}
