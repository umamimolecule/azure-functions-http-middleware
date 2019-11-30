using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public static class MiddlewarePipelineExtensions
    {
        public static IMiddlewarePipeline Use(this IMiddlewarePipeline pipeline, Func<HttpContext, Task<IActionResult>> func)
        {
            return pipeline.Use(new FunctionMiddleware(func));
        }

        public static IMiddlewarePipeline Use(this IMiddlewarePipeline pipeline, RequestDelegate requestDelegate)
        {
            return pipeline.Use(new RequestDelegateMiddleware(requestDelegate));
        }

        public static IMiddlewarePipeline UseCorrelationId(this IMiddlewarePipeline pipeline, IEnumerable<string> correlationIdHeaders)
        {
            return pipeline.Use(new CorrelationIdMiddleware(correlationIdHeaders));
        }

        public static IMiddlewarePipeline UseQueryValidation<T>(this IMiddlewarePipeline pipeline)
            where T : new()
        {
            return pipeline.Use(new QueryModelValidationMiddleware<T>());
        }

        public static IMiddlewarePipeline UseBodyValidation<T>(this IMiddlewarePipeline pipeline)
            where T: new()
        {
            return pipeline.Use(new BodyModelValidationMiddleware<T>());
        }
    }
}
