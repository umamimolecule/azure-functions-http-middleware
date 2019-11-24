using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umamimolecule.AzureFunctionsMiddleware;

namespace FunctionAppMiddlewarePOC
{
    public class MiddlewarePipelineFactory : IMiddlewarePipelineFactory
    {
        private readonly static string[] correlationIdHeaders = new string[]
        {
            "ms-request-id",
            "request-id"
        };

        public IMiddlewarePipeline CreateForQuery<TQuery>(Func<IHttpFunctionContext, Task<IActionResult>> func)
            where TQuery : new()
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline();
            return pipeline.UseCorrelationId(correlationIdHeaders)
                           .UseQueryValidation<TQuery>()
                           .Use(func);
        }

        public IMiddlewarePipeline CreateForBody<TBody>(Func<IHttpFunctionContext, Task<IActionResult>> func)
            where TBody : new()
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline();
            return pipeline.UseCorrelationId(correlationIdHeaders)
                           .UseBodyValidation<TBody>()
                           .Use(func);
        }

        public IMiddlewarePipeline CreateForQueryAndBody<TQuery, TBody>(Func<IHttpFunctionContext, Task<IActionResult>> func)
            where TQuery : new()
            where TBody : new()
        {
            MiddlewarePipeline pipeline = new MiddlewarePipeline();
            return pipeline.UseCorrelationId(correlationIdHeaders)
                           .UseQueryValidation<TQuery>()
                           .UseBodyValidation<TBody>()
                           .Use(func);
        }
    }
}
