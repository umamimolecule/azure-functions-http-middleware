using Microsoft.AspNetCore.Http;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class HttpContextAccessor : IHttpContextAccessor
    {
        public HttpContextAccessor(HttpContext context)
        {
            this.HttpContext = context;
        }

        public HttpContext HttpContext { get; set; }
    }
}
