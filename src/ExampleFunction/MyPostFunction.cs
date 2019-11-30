using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;
using System.Net.Http;

namespace FunctionAppMiddlewarePOC
{
    public class MyPostFunction
    {
        private readonly IMiddlewarePipeline pipeline;

        public MyPostFunction(IMiddlewarePipelineFactory pipelineFactory)
        {
            this.pipeline = pipelineFactory.CreateForBody<MyPostFunctionBodyParameters>(this.ExecuteAsync);
        }

        [FunctionName(nameof(MyPostFunction))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
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
                body = context.Items[ContextItems.Body]
            };

            return new OkObjectResult(payload);
        }
    }
}
