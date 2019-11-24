using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Middleware to perform validation body payload.
    /// </summary>
    /// <typeparam name="T">The body payload type.</typeparam>
    public class BodyModelValidationMiddleware<T> : HttpMiddleware
        where T: new()
    {
        public override async Task InvokeAsync(IHttpFunctionContext context)
        {
            var validationResult = await this.Validate(context);
            if (!validationResult.Success)
            {
                throw new BadRequestException(validationResult.Error);
            }

            context.BodyModel = validationResult.Model;

            if (this.Next != null)
            {
                await this.Next.InvokeAsync(context);
            }
        }

        private async Task<(bool Success, string Error, T Model)> Validate(IHttpFunctionContext context)
        {
            var model = await this.CreateModel(context);
            List<ValidationResult> validationResults = new List<ValidationResult>();

            if (!RecursiveValidator.TryValidateObject(model, validationResults, true))
            {
                return (false, string.Join(", ", validationResults.Select(x => x.ErrorMessage)), model);
            }

            return (true, null, model);
        }

        private async Task<T> CreateModel(IHttpFunctionContext context)
        {
            if (context.Request.Body != null)
            {
                StreamReader reader = new StreamReader(context.Request.Body);
                var json = await reader.ReadToEndAsync();
                if (context.Request.Body.CanSeek)
                {
                    context.Request.Body.Position = 0;
                }
                var model = JsonConvert.DeserializeObject<T>(json);
                if (model != null)
                {
                    return model;
                }
            }

            return new T();
        }
    }
}
