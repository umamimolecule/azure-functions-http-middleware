using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Samples.ModelValidation.Pipelines;
using Samples.ModelValidation.Responses;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.ModelValidation.Functions
{
    /// <summary>
    /// An HTTP-triggered Azure Function to demonstrate body payload parameter
    /// validation using middleware.
    /// </summary>
    public class BodyValidationWithCustomResponse
    {
        private readonly IMiddlewarePipeline pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyValidation"/> class.
        /// </summary>
        /// <param name="pipelineFactory">The middleware pipeline factory.</param>
        public BodyValidationWithCustomResponse(IMiddlewarePipelineFactory pipelineFactory)
        {
            // Note: You could simply new-up a MiddlewarePipeline instance here and build it,
            // but this example uses a pipeline factory so you can share pipelines between
            // Azure Functions that have common requirements, without having to duplicate
            // code.

            this.pipeline = pipelineFactory.CreateForBody<BodyValidationBody>(
                this.ExecuteAsync,
                ResponseHelper.HandleValidationFailure);
        }

        /// <summary>
        /// The HTTP trigger entrypoint for the function.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <returns></returns>
        [FunctionName(nameof(BodyValidationWithCustomResponse))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            return await this.pipeline.RunAsync();
        }

        /// <summary>
        /// Executes the function's business logic.  At this point, the body model
        /// has been validated correctly.
        /// </summary>
        /// <param name="context">The HTTP context for the request.</param>
        /// <returns>The <see cref="IActionResult"/> result.</returns>
        private Task<IActionResult> ExecuteAsync(HttpContext context)
        {
            var payload = new
            {
                correlationId = context.TraceIdentifier,
                message = "OK",
                body = context.Items[ContextItems.Body]
            };

            return Task.FromResult<IActionResult>(new OkObjectResult(payload));
        }
    }
}
