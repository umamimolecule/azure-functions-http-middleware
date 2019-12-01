using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Middleware to perform validation body payload.
    /// </summary>
    /// <typeparam name="T">The body payload type.</typeparam>
    public class BodyModelValidationMiddleware<T> : ValidationMiddleware<T>
        where T : new()
    {
        /// <summary>
        /// Gets the error code to use when validation fails.
        /// </summary>
        public override string ErrorCode => ErrorCodes.InvalidBody;

        /// <summary>
        /// Validates the body payload.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The validation results.</returns>
        protected override async Task<(bool Success, string Error, T Model)> ValidateAsync(HttpContext context)
        {
            (var model, var error) = await this.CreateModelAsync(context);
            if (!string.IsNullOrEmpty(error))
            {
                return (false, error, model);
            }

            List<ValidationResult> validationResults = new List<ValidationResult>();

            if (!RecursiveValidator.TryValidateObject(model, validationResults, true))
            {
                return (false, string.Join(", ", validationResults.Select(x => string.Join(", ", x.MemberNames) + ": " + x.ErrorMessage)), model);
            }

            context.Items[ContextItems.Body] = model;

            return (true, null, model);
        }

        private async Task<(T model, string error)> CreateModelAsync(HttpContext context)
        {
            if (context.Request.Body != null)
            {
                context.Request.EnableBuffering();
#pragma warning disable IDE0067 // Dispose objects before losing scope
                var reader = new StreamReader(context.Request.Body);
#pragma warning restore IDE0067 // Dispose objects before losing scope
                var json = await reader.ReadToEndAsync();
                if (context.Request.Body.CanSeek)
                {
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                }

                try
                {
                    var model = JsonConvert.DeserializeObject<T>(json);
                    if (model != null)
                    {
                        return (model, null);
                    }
                }
                catch (Exception e)
                {
                    return (default(T), e.Message);
                }
            }

            return (new T(), null);
        }
    }
}
