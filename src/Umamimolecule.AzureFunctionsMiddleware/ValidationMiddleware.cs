using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        /// Gets or sets a function to handle a validation failure and provide a custom response. If not set, a default response object will be sent.
        /// </summary>
        public Func<HttpContext, ModelValidationResult, IActionResult> HandleValidationFailure { get; set; }

        private Func<HttpContext, ModelValidationResult, IActionResult> DefaultFailureResponse => (HttpContext context, ModelValidationResult validationResult) =>
        {
            var response = new
            {
                correlationId = context.TraceIdentifier,
                error = new
                {
                    code = this.ErrorCode,
                    message = validationResult.Error,
                },
            };

            return new BadRequestObjectResult(response);
        };

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
                var result = this.HandleValidationFailure ?? this.DefaultFailureResponse;
                await context.ProcessActionResultAsync(result(context, validationResult));
            }
        }

        /// <summary>
        /// Validates the model.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The validation results.</returns>
        protected abstract Task<ModelValidationResult> ValidateAsync(HttpContext context);
    }
}
