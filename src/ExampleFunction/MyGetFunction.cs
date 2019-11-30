using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Umamimolecule.AzureFunctionsMiddleware;
using System.Net.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace FunctionAppMiddlewarePOC
{
    public class MyGetFunction
    {
        private readonly IMiddlewarePipeline pipeline;

        public MyGetFunction(IMiddlewarePipelineFactory pipelineFactory)
        {
            this.pipeline = pipelineFactory.CreateForQuery<MyGetFunctionQueryParameters>(this.ExecuteAsync);
        }

        [FunctionName(nameof(MyGetFunction))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            return await this.pipeline.RunAsync();
        }

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
