using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Middleware to perform validation of the query parameters.
    /// </summary>
    /// <typeparam name="T">The query parameter type.</typeparam>
    public class QueryModelValidationMiddleware<T> : HttpMiddleware
    {
        public override async Task InvokeAsync(IHttpFunctionContext context)
        {
            var validationResult = this.Validate(context);
            if (!validationResult.Success)
            {
                throw new BadRequestException(validationResult.Error);
            }

            context.QueryModel = validationResult.Model;

            if (this.Next != null)
            {
                await this.Next.InvokeAsync(context);
            }
        }

        private (bool Success, string Error, T Model) Validate(IHttpFunctionContext context)
        {
            var model = this.CreateModel(context);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true))
            {
                return (false, string.Join(", ", validationResults.Select(x => x.ErrorMessage)), model);
            }

            return (true, null, model);
        }

        private T CreateModel(IHttpFunctionContext context)
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

            return model;
        }
    }
}
