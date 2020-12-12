using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.PipelineBranching.Pipelines
{
    public class MiddlewareA : HttpMiddleware
    {
        public override async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["x-middleware-a"] = "Hello from middleware A";

            if (this.Next != null)
            {
                await this.Next.InvokeAsync(context);
            }
        }
    }
}
