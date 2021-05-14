using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Samples.PipelineBranching.Pipelines;

namespace Samples.PipelineBranching.Functions
{
    /// <summary>
    /// An HTTP-triggered Azure Function to demonstrate pipeline branching.
    /// </summary>
    public class Functions
    {
        private readonly IMiddlewarePipelineFactory pipelineFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Functions"/> class.
        /// </summary>
        /// <param name="pipelineFactory">The middleware pipeline factory.</param>
        public Functions(IMiddlewarePipelineFactory pipelineFactory)
        {
            this.pipelineFactory = pipelineFactory;
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
            var pipeline = this.pipelineFactory.Create(this.ExecuteFunction1Async);
            return await pipeline.RunAsync();
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
            var pipeline = this.pipelineFactory.Create(this.ExecuteFunction2Async);
            return await pipeline.RunAsync();
        }

        /// <summary>
        /// Executes the function 1's business logic.
        /// </summary>
        /// <param name="context">The HTTP context for the request.</param>
        /// <returns>The <see cref="IActionResult"/> result.</returns>
        private Task<IActionResult> ExecuteFunction1Async(HttpContext context)
        {
            var payload = new
            {
                message = "OK",
                functionName = "Function1"
            };

            return Task.FromResult<IActionResult>(new OkObjectResult(payload));
        }

        /// <summary>
        /// Executes the function 2's business logic.
        /// </summary>
        /// <param name="context">The HTTP context for the request.</param>
        /// <returns>The <see cref="IActionResult"/> result.</returns>
        private Task<IActionResult> ExecuteFunction2Async(HttpContext context)
        {
            var payload = new
            {
                message = "OK",
                functionName = "Function2"
            };

            return Task.FromResult<IActionResult>(new OkObjectResult(payload));
        }
    }
}
