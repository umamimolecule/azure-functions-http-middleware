using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Middleware to perform validation of the query parameters.
    /// </summary>
    /// <typeparam name="T">The query parameter type.</typeparam>
    public class QueryModelValidationMiddleware<T> : ValidationMiddleware<T>
        where T : new()
    {
        /// <summary>
        /// Gets the error code to use when validation fails.
        /// </summary>
        public override string ErrorCode => ErrorCodes.InvalidQueryParameters;

        /// <summary>
        /// Validates the query parameters.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The validation results.</returns>
        protected override async Task<(bool Success, string Error, T Model)> ValidateAsync(HttpContext context)
        {
            var model = await this.CreateModelAsync(context);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true))
            {
                return (false, string.Join(", ", validationResults.Select(x => x.ErrorMessage)), model);
            }

            context.Items[ContextItems.Query] = model;

            return (true, null, model);
        }

        private Task<T> CreateModelAsync(HttpContext context)
        {
            var type = typeof(T);
            var ctor = type.GetConstructor(System.Type.EmptyTypes);
            if (ctor == null)
            {
                throw new ApplicationException($"Could not find a parameterless public constructor for the type {type.FullName}");
            }

            var model = (T)ctor.Invoke(null);

            if (context.Request.Query != null)
            {
                var properties = type.GetProperties();
                foreach (var parameter in context.Request.Query)
                {
                    var property = properties.FirstOrDefault(x => string.Compare(x.Name, parameter.Key, true) == 0);
                    if (property != null)
                    {
                        property.SetValue(model, parameter.Value.ToString());
                    }
                }
            }

            return Task.FromResult(model);
        }
    }
}
