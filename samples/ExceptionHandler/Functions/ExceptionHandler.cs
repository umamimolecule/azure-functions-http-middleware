using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;
using Samples.ModelValidation.Pipelines;
using Samples.ModelValidation.Exceptions;

namespace Samples.ModelValidation.Functions
{
    /// <summary>
    /// An HTTP-triggered Azure Function to demonstrate exception handling middleware.
    /// </summary>
    public class ExceptionHandler
    {
        private readonly IMiddlewarePipeline pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyValidation"/> class.
        /// </summary>
        /// <param name="pipelineFactory">The middleware pipeline factory.</param>
        public ExceptionHandler(IMiddlewarePipelineFactory pipelineFactory)
        {
            this.pipeline = pipelineFactory.Create(this.ExecuteAsync);
        }

        /// <summary>
        /// The HTTP trigger entrypoint for the function.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <returns></returns>
        [FunctionName(nameof(ExceptionHandler))]
        public async Task<IActionResult> Run(
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
        private Task<IActionResult> ExecuteAsync(HttpContext context)
        {
            // Throw an exception to test to handler
            throw new ThrottledException(TimeSpan.FromMinutes(5));
        }
    }
}
