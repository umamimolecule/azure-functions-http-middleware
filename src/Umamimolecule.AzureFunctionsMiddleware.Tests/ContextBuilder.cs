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

        public ContextBuilder Accepts(string contentType)
        {
            this.context.Request.Headers["Accepts"] = contentType;
            return this;
        }

        public ContextBuilder AddServiceProvider(IServiceProvider serviceProvider)
        {
            this.context.RequestServices = serviceProvider;
            return this;
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
