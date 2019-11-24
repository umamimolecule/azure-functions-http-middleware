using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public class MiddlewarePipeline : IMiddlewarePipeline
    {
        private readonly List<HttpMiddleware> pipeline;

        public MiddlewarePipeline()
        {
            this.pipeline = new List<HttpMiddleware>();
        }

        public Func<Exception, IHttpFunctionContext, Task<IActionResult>> ExceptionHandler { get; set; }

        public IMiddlewarePipeline Use(HttpMiddleware middleware)
        {
            if (pipeline.Any())
            {
                pipeline.Last().Next = middleware;
            }

            pipeline.Add(middleware);

            return this;
        }

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
                // TODO: Log exception

                var handler = this.ExceptionHandler ?? this.DefaultExceptionHandler;
                return await handler(ex, context);
            }
        }

        private Task<IActionResult> DefaultExceptionHandler(Exception ex, IHttpFunctionContext context)
        {
            IActionResult result;

            if (ex is BadRequestException)
            {
                dynamic response = new
                {
                    correlationId = context.CorrelationId,
                    error = new
                    {
                        code = "BAD_REQUEST",
                        message = ex.Message
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
