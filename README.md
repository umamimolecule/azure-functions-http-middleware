![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/umamimolecule/azure-functions-http-middleware/14/master) ![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/umamimolecule/azure-functions-http-middleware/14/master)

# azure-functions-http-middleware

An extensible middleware implementation for HTTP-triggered Azure Functions in .Net.

## NuGet Package

https://www.nuget.org/packages/Umamimolecule.AzureFunctionsMiddleware/

## Introduction

It lets you do stuff like this in your function:

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

After having written several HTTP-triggered Azure Functions and writing the same cross-cutting concerns over and over for model validation, error handling, correlation IDs and such, it seemed appropriate to bundle all this into a package that can be re-used.

This project was inspired by [this blog post](https://dasith.me/2018/01/20/using-azure-functions-httptrigger-as-web-api/) by Dasith Wijesiriwardena.

## Dependencies
- Azure Functions 1.0.29
- .Net Standard 2.0

## Getting Started

1. Run the following command in NuGet Package Manager Console (targetting your Azure Function project):
```
install-package Umamimolecule.AzureFunctionsMiddleware
```

2. Set up your pipeline in your Azure Function

_Note: Middleware pipelines are configured within each Azure Function's constructor.  This is unlike ASP.Net Core where the pipelines are defined within the Startup class, and this is due to the way the Azure functions runtime works where it does not expose any `IApplicationBuilder` type of bootstrapping._
```
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Umamimolecule.AzureFunctionsMiddleware;

namespace FunctionAppMiddlewarePOC
{
    public class MyGetFunction
    {
        private readonly IMiddlewarePipeline pipeline;

        public MyGetFunction()
        {
            this.pipeline = new MiddlewarePipeline();
            this.pipeline.UseCorrelationId(correlationIdHeaders)
                         .UseQueryValidation<TQuery>()
                         .Use(this.ExecuteAsync);            
        }
    }
}
```

3. In your HTTP trigger function, execute your pipeline:
```
[FunctionName(nameof(MyGetFunction))]
public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
{
    return await this.pipeline.RunAsync();
}

private async Task<IActionResult> ExecuteAsync(HttpContext context)
{
    // Your function logic goes here...
}
```
## Samples

See the [Samples](https://github.com/umamimolecule/azure-functions-http-middleware/tree/master/samples) folder for some example use-cases.

## Built-in middleware
This package comes with the following built-in middleware:

### BodyModelValidationMiddleware
Validates the body model for the request.  If successful, the body will be available in `HttpContext.Items["Body"]`.

### CorrelationIdMiddleware
Extracts a correlation ID from the request headers and sets the value to `HttpContext.TraceIdentifier`.  You can specify a collection of correlation ID header names and the first matching header will be used.  If no matching headers are found, a unique GUID will be used.

### ExceptionHandlerMiddleware
Allows exceptions to be handled and a custom response to be returned.

### FunctionMiddleware
Intended for your Azure Function implementation.

### QueryModelValidationMiddleware
Validates the query model for the request.  If successful, the query object will be available in `HttpContext.Items["Query"]`.

### RequestDelegateMiddleware
A general-purpose middleware for `RequestDelegate` instances.

## Creating your own middleware
You can implement `IHttpMiddleware` or sub-class the `HttpMiddleware` abstract class. Here's an example of some middleware to add a response header `x-request-date-utc` which contains the current UTC date and time of the request:
```
public class UtcRequestDateMiddleWare : HttpMiddleware
{
    public override Task InvokeAsync(HttpContext context)
    {
       context.Response.Headers["x-request-date-utc"] = System.DateTime.UtcNow.ToString("o");
    }
}
```
