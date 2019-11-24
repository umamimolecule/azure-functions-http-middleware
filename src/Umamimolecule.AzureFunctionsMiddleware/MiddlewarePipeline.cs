using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// The middleware pipeline.
    /// </summary>
    public class MiddlewarePipeline : IMiddlewarePipeline
    {
        private readonly List<IHttpMiddleware> pipeline = new List<IHttpMiddleware>();

        /// <summary>
        /// Gets or sets the exception handler.  Allows you to control how exceptions are handled and
        /// what status code and payload is returned in the response.
        /// </summary>
        public Func<Exception, IHttpFunctionContext, Task<IActionResult>> ExceptionHandler { get; set; }

        /// <summary>
        /// Adds middleware to the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        /// <returns>The pipeline.</returns>
        public IMiddlewarePipeline Use(IHttpMiddleware middleware)
        {
            if (pipeline.Any())
            {
                pipeline.Last().Next = middleware;
            }

            pipeline.Add(middleware);

            return this;
        }

        /// <summary>
        /// Executes the pipeline.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The value to returned from the Azure function.</returns>
        public async Task<IActionResult> RunAsync(HttpRequest request)
        {
            var context = new HttpFunctionContext()
            {
                Request = request
            };

            try
            {
                if (pipeline.Any())
                {
                    await pipeline.First().InvokeAsync(context);

                    if (context.Response != null)
                    {
                        return context.Response;
                    }
                }

                throw new MiddlewarePipelineException();
            }
            catch (Exception ex)
            {
                var handler = this.ExceptionHandler ?? this.DefaultExceptionHandler;
                return await handler(ex, context);
            }
        }

        /// <summary>
        /// A default exception handler to provide basic support for model validation failure and unexpected exceptions.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="context">The function execution context.</param>
        /// <returns>The response to return from the Azure function.</returns>
        private Task<IActionResult> DefaultExceptionHandler(Exception exception, IHttpFunctionContext context)
        {
            IActionResult result;

            if (exception is BadRequestException)
            {
                dynamic response = new
                {
                    correlationId = context.CorrelationId,
                    error = new
                    {
                        code = "BAD_REQUEST",
                        message = exception.Message
                    }
                };

                result = new BadRequestObjectResult(response);
            }
            else
            {
                dynamic response = new
                {
                    correlationId = context.CorrelationId,
                    error = new
                    {
                        code = "INTERNAL_SERVER_ERROR",
                        message = "An unexpected error occurred in the application"
                    }
                };

                result = new ObjectResult(response)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return Task.FromResult(result);
        }
    }
}
