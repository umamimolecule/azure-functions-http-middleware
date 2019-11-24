using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public interface IMiddlewarePipelineFactory
    {
        IMiddlewarePipeline Create(Func<IHttpFunctionContext, Task<IActionResult>> func);
    }
}
