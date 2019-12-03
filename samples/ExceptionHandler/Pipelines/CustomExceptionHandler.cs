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
        public static async Task<IActionResult> HandleAsync(Exception exception, HttpContext context)
        {
            await Task.CompletedTask;

            switch (exception)
            {
                case ThrottledException t:
                    context.Response.Headers["Retry-After"] = ((int)t.TryAgain.TotalSeconds).ToString();
                    return new StatusCodeResult(429);

                default:
                    dynamic body = new
                    {
                        correlationId = context.TraceIdentifier,
                        error = new
                        {
                            code = "INTERNAL_SERVER_ERROR",
                            message = "An unexpected error occurred."
                        }
                    };

                    return new ObjectResult(body)
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
            }
        }
    }
}
