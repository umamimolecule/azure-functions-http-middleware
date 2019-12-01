using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Contains extension methods for <see cref="HttpContext"/> instances.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Processes an action result and applies to the <see cref="HttpContext"/> instance.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="result">The aciton result to process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task ProcessActionResultAsync(this HttpContext context, IActionResult result)
        {
            await result.ExecuteResultAsync(new ActionContext(context, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));
        }
    }
}
