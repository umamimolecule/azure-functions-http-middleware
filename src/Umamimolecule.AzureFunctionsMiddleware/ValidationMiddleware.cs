using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    public abstract class ValidationMiddleware<T> : HttpMiddleware
        where T : new()
    {
        public abstract string ErrorCode { get; }

        public override async Task InvokeAsync(HttpContext context)
        {
            var validationResult = await this.ValidateAsync(context);
            if (validationResult.Success)
            {
                if (this.Next != null)
                {
                    await this.Next.InvokeAsync(context);
                }
            }
            else
            {
                dynamic response = new
                {
                    correlationId = context.TraceIdentifier,
                    error = new
                    {
                        code = this.ErrorCode,
                        message = validationResult.Error
                    }
                };

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = ContentTypes.ApplicationJson;
                await HttpResponseWritingExtensions.WriteAsync(context.Response, JsonConvert.SerializeObject(response));
            }
        }

        protected abstract Task<(bool Success, string Error, T Model)> ValidateAsync(HttpContext context);
    }
}
