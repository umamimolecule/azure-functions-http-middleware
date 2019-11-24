using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public class CorrelationIdMiddleware : HttpMiddleware
    {
        private readonly IEnumerable<string> correlationIdHeaders;

        public CorrelationIdMiddleware(IEnumerable<string> correlationIdHeaders)
        {
            this.correlationIdHeaders = correlationIdHeaders;
        }

        public override async Task InvokeAsync(IHttpFunctionContext context)
        {
            context.CorrelationId = this.GetCorrelationId(context.Request);

            if (this.Next != null)
            {
                await this.Next.InvokeAsync(context);
            }
        }

        private string GetCorrelationId(HttpRequest request)
        {
            foreach (var correlationIdHeader in this.correlationIdHeaders)
            {
                if (request.Headers.TryGetValue(correlationIdHeader, out StringValues value) && !StringValues.IsNullOrEmpty(value))
                {
                    return value.ToString();
                }
            }

            return Guid.NewGuid().ToString();
        }
    }
}
