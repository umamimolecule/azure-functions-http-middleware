using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Umamimolecule.AzureFunctionsMiddleware;
using Samples.ConditionalBranching.Pipelines;

namespace Samples.ConditionalBranching.Functions
{
    /// <summary>
    /// An HTTP-triggered Azure Function to demonstrate conditional branching.
    /// </summary>
    public class Function
    {
        private readonly IMiddlewarePipeline pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyValidation"/> class.
        /// </summary>
        /// <param name="pipelineFactory">The middleware pipeline factory.</param>
        public Function(IMiddlewarePipelineFactory pipelineFactory)
        {
            // Note: You could simply new-up a MiddlewarePipeline instance here and build it,
            // but this example uses a pipeline factory so you can share pipelines between
            // Azure Functions that have common requirements, without having to duplicate
            // code.

            this.pipeline = pipelineFactory.Create(this.ExecuteAsync);
        }

        /// <summary>
        /// The HTTP trigger entrypoint for the first function.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <returns></returns>
        [FunctionName("Function1")]
        public async Task<IActionResult> Function1(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            return await this.pipeline.RunAsync();
        }

        /// <summary>
        /// The HTTP trigger entrypoint for the second function.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <returns></returns>
        [FunctionName("Function2")]
        public async Task<IActionResult> Function2(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            return await this.pipeline.RunAsync();
        }

        /// <summary>
        /// Executes the function's business logic.  At this point, the body model
        /// has been validated correctly.
        /// </summary>
        /// <param name="context">The HTTP context for the request.</param>
        /// <returns>The <see cref="IActionResult"/> result.</returns>
        private async Task<IActionResult> ExecuteAsync(HttpContext context)
        {
            await Task.CompletedTask;

            dynamic payload = new
            {
                message = "OK"
            };

            return new OkObjectResult(payload);
        }
    }
}
