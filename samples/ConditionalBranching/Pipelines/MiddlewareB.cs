using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.ConditionalBranching.Pipelines
{
    public class MiddlewareB : HttpMiddleware
    {
        public override Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["x-middleware-b"] = "Hello from middleware B";
            return Task.CompletedTask;
        }
    }
}
