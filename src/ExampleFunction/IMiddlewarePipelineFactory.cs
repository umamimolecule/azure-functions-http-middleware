using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umamimolecule.AzureFunctionsMiddleware;

namespace FunctionAppMiddlewarePOC
{
    public interface IMiddlewarePipelineFactory
    {
        IMiddlewarePipeline CreateForQuery<TQuery>(Func<IHttpFunctionContext, Task<IActionResult>> func) where TQuery : new();

        IMiddlewarePipeline CreateForBody<TBody>(Func<IHttpFunctionContext, Task<IActionResult>> func) where TBody : new();

        IMiddlewarePipeline CreateForQueryAndBody<TQuery, TBody>(Func<IHttpFunctionContext, Task<IActionResult>> func) where TQuery : new() where TBody : new();
    }
}
