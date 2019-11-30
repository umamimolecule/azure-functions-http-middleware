using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public static class HttpContextExtensions
    {
        public static async Task ProcessActionResultAsync(this HttpContext context, IActionResult result)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
