using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public class ContextBuilder
    {
        private readonly HttpContext context;

        public ContextBuilder()
        {
            this.context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream();
            context.Response.Body = new MemoryStream();
        }

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

        public ContextBuilder AddQuery(IQueryCollection query)
        {
            this.context.Request.Query = query;
            return this;
        }

        public ContextBuilder AddJsonBody<T>(T body)
        {
            StreamWriter writer = new StreamWriter(this.context.Request.Body);
            writer.Write(JsonConvert.SerializeObject(body));
            writer.Flush();
            this.context.Request.Body.Seek(0, SeekOrigin.Begin);
            this.context.Request.ContentType = "application/json";
            return this;
        }
    }
}
