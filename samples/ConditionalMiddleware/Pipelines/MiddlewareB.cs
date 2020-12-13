using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.ConditionalMiddleware.Pipelines
{
    public class MiddlewareB : HttpMiddleware
    {
        public override async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["x-middleware-b"] = "Hello from middleware B";

            if (this.Next != null)
            {
                await this.Next.InvokeAsync(context);
            }
        }
    }
}
