using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Samples.ModelValidation.Exceptions;

namespace Samples.ModelValidation.Pipelines
{
    class CustomExceptionHandler
    {
        public static Task<IActionResult> HandleAsync(Exception exception, HttpContext context)
        {
            IActionResult result;
            switch (exception)
            {
                case ThrottledException t:
                    context.Response.Headers["Retry-After"] = ((int)t.TryAgain.TotalSeconds).ToString();
                    result = new StatusCodeResult(429);
                    break;

                default:
                    var body = new
                    {
                        correlationId = context.TraceIdentifier,
                        error = new
                        {
                            code = "INTERNAL_SERVER_ERROR",
                            message = "An unexpected error occurred."
                        }
                    };

                    result = new ObjectResult(body)
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                    break;
            }

            return Task.FromResult<IActionResult>(result);
        }
    }
}
