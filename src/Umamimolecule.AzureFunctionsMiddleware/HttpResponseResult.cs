using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public class HttpResponseResult : IActionResult
    {
        private readonly HttpContext context;

        public HttpResponseResult(HttpContext context)
        {
            this.context = context;
        }

        public HttpContext Context => this.context;

        public async Task ExecuteResultAsync(ActionContext context)
        {
            await Task.CompletedTask;
            context.HttpContext = this.context;
        }
    }
}
