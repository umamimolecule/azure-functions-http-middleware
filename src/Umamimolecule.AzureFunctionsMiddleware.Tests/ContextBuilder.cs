using System;
using Microsoft.AspNetCore.Http;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class ContextBuilder
    {
        private readonly HttpContext context = new DefaultHttpContext();

        public HttpContext Build()
        {
            return this.context;
        }

        public ContextBuilder AddRequestHeaders(IHeaderDictionary headers)
        {
            foreach (var header in headers)
            {
                this.context.Request.Headers.Add(header.Key, header.Value);
            }

            return this;
        }
    }
}
