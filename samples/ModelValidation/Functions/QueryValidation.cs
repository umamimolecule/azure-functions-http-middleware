using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Samples.ModelValidation.Pipelines;
using Umamimolecule.AzureFunctionsMiddleware;

namespace Samples.ModelValidation.Functions
{
    /// <summary>
    /// An HTTP-triggered Azure Function to demonstrate query parameter validation
    /// using middleware.
    /// </summary>
    public class QueryValidation
    {
        private readonly IMiddlewarePipeline pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryValidation"/> class.
        /// </summary>
        /// <param name="pipelineFactory">The middleware pipeline factory.</param>
        public QueryValidation(IMiddlewarePipelineFactory pipelineFactory)
        {
            // Note: You could simply new-up a MiddlewarePipeline instance here and build it,
            // but this example uses a pipeline factory so you can share pipelines between
            // Azure Functions that have common requirements, without having to duplicate
            // code.

            this.pipeline = pipelineFactory.CreateForQuery<QueryValidationQueryParameters>(this.ExecuteAsync);
        }

        /// <summary>
        /// The HTTP trigger entrypoint for the function.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <returns>The <see cref="IActionResult"/> result.</returns>
        [FunctionName(nameof(QueryValidation))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            // Note that we don't need to use HTtpRequest parameter here as the
            // pipeline is using IHttpContextAccessor.
            return await this.pipeline.RunAsync();
        }

        /// <summary>
        /// Executes the function's business logic.  At this point, the query model
        /// has been validated correctly.
        /// </summary>
        /// <param name="context">The HTTP context for the request.</param>
        /// <returns>The <see cref="IActionResult"/> result.</returns>
        private async Task<IActionResult> ExecuteAsync(HttpContext context)
        {
            await Task.CompletedTask;

            dynamic payload = new
            {
                correlationId = context.TraceIdentifier,
                message = "OK",
                query = context.Items[ContextItems.Query]
            };

            return new OkObjectResult(payload);
        }
    }
}
