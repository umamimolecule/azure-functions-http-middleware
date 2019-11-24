using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public interface IMiddlewarePipeline
    {
        IMiddlewarePipeline Use(HttpMiddleware middleware);
        
        Task<IActionResult> RunAsync(HttpRequest context);

        Func<Exception, IHttpFunctionContext, Task<IActionResult>> ExceptionHandler { get; set; }
    }
}
