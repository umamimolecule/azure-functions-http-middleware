using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.PipelineBranching.Pipelines
{
    public class MiddlewareA : HttpMiddleware
    {
        public override Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["x-middleware-a"] = "Hello from middleware A";
            return Task.CompletedTask;
        }
    }
}
