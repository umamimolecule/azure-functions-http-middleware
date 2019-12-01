using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Base class for validation middleware.
    /// </summary>
    /// <typeparam name="T">The object type containing the model to validate.</typeparam>
    public abstract class ValidationMiddleware<T> : HttpMiddleware
        where T : new()
    {
        /// <summary>
        /// Gets the error code to use when validation fails.
        /// </summary>
        public abstract string ErrorCode { get; }

        /// <summary>
        /// Runs the middleware.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task InvokeAsync(HttpContext context)
        {
#pragma warning disable IDE0042 // Deconstruct variable declaration
            var validationResult = await this.ValidateAsync(context);
#pragma warning restore IDE0042 // Deconstruct variable declaration
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
                        message = validationResult.Error,
                    },
                };

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = ContentTypes.ApplicationJson;
                await HttpResponseWritingExtensions.WriteAsync(context.Response, JsonConvert.SerializeObject(response));
            }
        }

        /// <summary>
        /// Validates the model.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The validation results.</returns>
        protected abstract Task<(bool Success, string Error, T Model)> ValidateAsync(HttpContext context);
    }
}
