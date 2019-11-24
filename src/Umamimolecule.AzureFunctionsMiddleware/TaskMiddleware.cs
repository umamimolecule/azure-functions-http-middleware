using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Middleware to allow an asynchronous operation to be executed.
    /// </summary>
    public class TaskMiddleware : HttpMiddleware
    {
        private readonly Func<IHttpFunctionContext, Task<IActionResult>> func;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskMiddleware"/> class.
        /// </summary>
        /// <param name="func">The task to be executed.</param>
        public TaskMiddleware(Func<IHttpFunctionContext, Task<IActionResult>> func)
        {
            this.func = func;
        }

        public override async Task InvokeAsync(IHttpFunctionContext context)
        {
            context.Response = await func(context);
        }
    }
}
