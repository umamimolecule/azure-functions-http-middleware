using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public class HttpResponseResult : IActionResult
    {
        private readonly HttpResponse response;

        public HttpResponseResult(HttpResponse response)
        {
            this.response = response;
        }

        public HttpResponse Response => this.response;

        public Task ExecuteResultAsync(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
