[![Build Status](https://umamimolecule.visualstudio.com/UmamiTools/_apis/build/status/Umamimolecule.AzureFunctionsMiddleware%20CI?branchName=master)](https://umamimolecule.visualstudio.com/UmamiTools/_build/latest?definitionId=13&branchName=master) ![Azure DevOps coverage (master)](https://img.shields.io/azure-devops/coverage/umamimolecule/UmamiTools/13/master)

# azure-functions-http-middleware

An extensible middleware implementation for HTTP-triggered Azure Functions in .Net.

Lets you do stuff like this in your function:

```
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;

namespace MyFunctionApp
{
    public class MyFunction
    {
        private readonly IMiddlewarePipeline pipeline;

        public MyFunction()
        {
            // This pipeline will:
            // 1. Extract correlation ID from request header 'request-id' and put into HttpContext.TraceIdentifier,
            // 2. Validate that required query parameters are present and put into HttpContext.Items["Query"]
            // 3. Validate the body payload contains all mandatory fields and put into HttpContext.Items["Body"]
            // 4. Executes the logic for this Azure Function
            //
            // Any validation errors will result in a 400 Bad Request returned.
            
            this.pipeline = new MiddlewarePipeline();
            this.pipeline.UseCorrelationId(new string[] { "request-id" } )
                         .UseQueryValidation<QueryParameters>()
                         .UseBodyValidation<BodyPayload>()
                         .Use(this.ExecuteAsync);
        }

        [FunctionName(nameof(MyFunction))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            return await this.pipeline.RunAsync(req);
        }

        private async Task<IActionResult> ExecuteAsync(HttpContext context)
        {
            // At this point, the query and body payloads have been validated, and
            // the correlation ID has been extracted from request headers.
            
            await Task.CompletedTask;

            dynamic payload = new
            {
                correlationId = context.TraceIdentifier,
                body = context.Items[ContextItems.Body],
                query = context.Items[ContextItems.Query]
            };

            return new OkObjectResult(payload);
        }
    }

    public class QueryParameters
    {
        [Required]
        public string Id { get; set; }
    }

    public class BodyPayload
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
```

## Motivation

After having written several HTTP-triggered Azure Functions and writing the same cross-cutting concerns over and over for model validation, error handling, correlation IDs and such, it seemed appropriate to bundle all this into a package that can be re-used

This project was inspired by [this blog post](https://dasith.me/2018/01/20/using-azure-functions-httptrigger-as-web-api/) by Dasith Wijesiriwardena.

## Dependencies
- Azure Functions 1.0.29
- .Net Standard 2.0

