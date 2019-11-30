using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public static class HttpContextExtensions
    {
        public static async Task ProcessActionResultAsync(this HttpContext context, IActionResult result)
        {
            await result.ExecuteResultAsync(new ActionContext(context, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));
        }
    }
}
