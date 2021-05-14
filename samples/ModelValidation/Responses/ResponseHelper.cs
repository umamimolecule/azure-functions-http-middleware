using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.ModelValidation.Responses
{
    /// <summary>
    /// Contains utility methods related to responses.
    /// </summary>
    public static class ResponseHelper
    {
        /// <summary>
        /// Example of a custom response handler for valiation failures.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="validationResult"></param>
        /// <returns></returns>
        public static IActionResult HandleValidationFailure(
            HttpContext _,
            ModelValidationResult validationResult)
        {
            var response = new
            {
                customResponse = new
                {
                    errorMessage = validationResult.Error
                }
            };

            return new BadRequestObjectResult(response);
        }
    }
}
