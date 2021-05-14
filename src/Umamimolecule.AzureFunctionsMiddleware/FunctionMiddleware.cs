using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Middleware to allow an asynchronous operation to be executed.
    /// </summary>
    public class FunctionMiddleware : HttpMiddleware
    {
        private readonly Func<HttpContext, Task<IActionResult>> func;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionMiddleware"/> class.
        /// </summary>
        /// <param name="func">The task to be executed.</param>
        public FunctionMiddleware(Func<HttpContext, Task<IActionResult>> func)
        {
            GuardClauses.IsNotNull(nameof(func), func);
            this.func = func;
        }

        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task InvokeAsync(HttpContext context)
        {
            var result = await this.func(context);
            await context.ProcessActionResultAsync(result);
        }
    }
}
