using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umamimolecule.AzureFunctionsMiddleware;

namespace FunctionAppMiddlewarePOC
{
    public interface IMiddlewarePipelineFactory
    {
        IMiddlewarePipeline CreateForQuery<TQuery>(Func<HttpContext, Task<IActionResult>> func) where TQuery : new();

        IMiddlewarePipeline CreateForBody<TBody>(Func<HttpContext, Task<IActionResult>> func) where TBody : new();

        IMiddlewarePipeline CreateForQueryAndBody<TQuery, TBody>(Func<HttpContext, Task<IActionResult>> func) where TQuery : new() where TBody : new();
    }
}
