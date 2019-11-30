using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umamimolecule.AzureFunctionsMiddleware;

namespace FunctionAppMiddlewarePOC
{
    public class MiddlewarePipelineFactory : IMiddlewarePipelineFactory
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        private readonly static string[] correlationIdHeaders = new string[]
        {
            "ms-request-id",
            "request-id"
        };

        public MiddlewarePipelineFactory(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public IMiddlewarePipeline CreateForQuery<TQuery>(Func<HttpContext, Task<IActionResult>> func)
            where TQuery : new()
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline(this.httpContextAccessor);
            return pipeline.UseCorrelationId(correlationIdHeaders)
                           .UseQueryValidation<TQuery>()
                           .Use(func);
        }

        public IMiddlewarePipeline CreateForBody<TBody>(Func<HttpContext, Task<IActionResult>> func)
            where TBody : new()
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline(this.httpContextAccessor);
            return pipeline.UseCorrelationId(correlationIdHeaders)
                           .UseBodyValidation<TBody>()
                           .Use(func);
        }

        public IMiddlewarePipeline CreateForQueryAndBody<TQuery, TBody>(Func<HttpContext, Task<IActionResult>> func)
            where TQuery : new()
            where TBody : new()
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline(this.httpContextAccessor);
            return pipeline.UseCorrelationId(correlationIdHeaders)
                           .UseQueryValidation<TQuery>()
                           .UseBodyValidation<TBody>()
                           .Use(func);
        }
    }
}
