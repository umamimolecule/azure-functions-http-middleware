using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public class AbstractFunctionMiddleware : HttpMiddleware
    {
        private readonly Func<IHttpFunctionContext, Task<IActionResult>> func;

        public AbstractFunctionMiddleware(Func<IHttpFunctionContext, Task<IActionResult>> func)
        {
            this.func = func;
        }

        public override async Task InvokeAsync(IHttpFunctionContext context)
        {
            context.Response = await func(context);
        }
    }
}
