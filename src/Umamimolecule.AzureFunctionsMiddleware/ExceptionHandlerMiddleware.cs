using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Exception handling middleware.
    /// </summary>
    public class ExceptionHandlerMiddleware : HttpMiddleware
    {
        /// <summary>
        /// Gets a default exception handler to provide basic support for model validation failure and unexpected exceptions.
        /// </summary>
        /// <returns>The response to return from the Azure function.</returns>
        public static Func<Exception, HttpContext, Task<IActionResult>> DefaultExceptionHandler
        {
            get
            {
                return (Exception exception, HttpContext context) =>
                {
                    IActionResult result;

                    if (exception is BadRequestException)
                    {
                        dynamic response = new
                        {
                            correlationId = context.TraceIdentifier,
                            error = new
                            {
                                code = ErrorCodes.BadRequest,
                                message = exception.Message,
                            },
                        };

                        result = new BadRequestObjectResult(response);
                    }
                    else
                    {
                        dynamic response = new
                        {
                            correlationId = context.TraceIdentifier,
                            error = new
                            {
                                code = ErrorCodes.InternalServerError,
                                message = "An unexpected error occurred in the application",
                            },
                        };

                        result = new ObjectResult(response)
                        {
                            StatusCode = (int)HttpStatusCode.InternalServerError,
                        };
                    }

                    return Task.FromResult(result);
                };
            }
        }

        /// <summary>
        /// Gets or sets the function to determine what response should be returned by the exception handler.
        /// </summary>
        public Func<Exception, HttpContext, Task<IActionResult>> ExceptionHandler { get; set; }

        /// <summary>
        /// Gets or sets the handler function to log exceptions.  Optional.
        /// </summary>
        public Func<Exception, Task> LogExceptionAsync { get; set; }

        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task InvokeAsync(HttpContext context)
        {
            if (this.Next == null)
            {
                throw new MiddlewarePipelineException($"{this.GetType().FullName} must have a Next middleware");
            }

            try
            {
                await this.Next.InvokeAsync(context);
            }
            catch (Exception e)
            {
                if (this.LogExceptionAsync != null)
                {
                    await this.LogExceptionAsync(e);
                }

                if (this.ExceptionHandler == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.Headers[Headers.ContentType] = ContentTypes.ApplicationJson;
                    dynamic content = new
                    {
                        error = new
                        {
                            code = ErrorCodes.InternalServerError,
                            message = "An internal server error occurred",
                        },
                        correlationId = context.TraceIdentifier,
                    };

                    await HttpResponseWritingExtensions.WriteAsync(context.Response, JsonConvert.SerializeObject(content));
                }
                else
                {
                    var result = await this.ExceptionHandler(e, context);
                    await context.ProcessActionResultAsync(result);
                }
            }
        }
    }
}
